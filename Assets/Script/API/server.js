// server.js (ES Module)
import dotenv from "dotenv";
dotenv.config();

import express from "express";
import mysql from "mysql2/promise";
import cors from "cors";
import axios from "axios";
import { WebSocketServer } from "ws";
import { createServer } from "http";

const app = express();
const PORT = 5000;

// 1) เปิดใช้งาน CORS, JSON Parser
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// สร้าง HTTP Server สำหรับ Express API
const server = createServer(app);

// สร้าง WebSocket Server (หากต้องการใช้งาน)
const wss = new WebSocketServer({ port: 8080 });
wss.on("connection", (ws) => {
  console.log("Unity Connected via WebSocket");
  ws.send("Connected to WebSocket Server");
});

// ฟังก์ชันแจ้งเตือน Unity ผ่าน WebSocket (ปรับให้ส่ง userId ไปด้วย)
function notifyUnity(token, userId) {
  wss.clients.forEach((client) => {
    if (client.readyState === 1) {
      // ส่งเป็น JSON ที่มีทั้ง accessToken และ userId
      client.send(JSON.stringify({ accessToken: token, userId: userId }));
    }
  });
}

// -----------------------------------------------------------
// สร้าง Connection Pool ของ MySQL
const db = mysql.createPool({
  host: "localhost",
  user: "root",
  password: "boomza532",
  database: "project_circuit",
  waitForConnections: true,
  connectionLimit: 10,
  queueLimit: 0,
});

// -----------------------------------------------------------
// ทดสอบเชื่อมต่อ DB (Optional)
(async function testDB() {
  try {
    const conn = await db.getConnection();
    console.log("Connected to MySQL (Connection Pool)");
    conn.release();
  } catch (error) {
    console.error("Cannot connect to MySQL:", error);
  }
})();

// -----------------------------------------------------------
// Google OAuth Callback & Logout
// -----------------------------------------------------------
app.get("/callback", (req, res) => {
  res.send(`
    <script>
      const hash = window.location.hash.substring(1);
      const params = new URLSearchParams(hash);
      const token = params.get("access_token");

      if (token) {
          fetch("http://localhost:5000/register", {
              method: "POST",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify({ accessToken: token })
          })
          .then(response => response.json())
          .then(data => {
              console.log("Login Success:", data);
              // แจ้ง Unity ผ่าน WebSocket
              fetch("http://localhost:8080/notify", {
                  method: "POST",
                  headers: { "Content-Type": "application/json" },
                  body: JSON.stringify({ accessToken: token })
              });
              // ส่ง deep link กลับไปให้ Unity
              window.location.href = "unitydl://auth?access_token=" + token;
              setTimeout(() => { window.open('', '_self', ''); window.close(); }, 1000);
          })
          .catch(error => {
              console.error("Error:", error);
              window.location.href = "http://localhost:5000/error";
          });
      } else {
          window.location.href = "http://localhost:5000/error";
      }
    </script>
  `);
});

app.get("/error", (req, res) => {
  res.send("<h1>Error</h1><p>Authentication failed. Please try again.</p>");
});

app.post("/register", async (req, res) => {
  const { accessToken } = req.body;
  if (!accessToken) {
    console.error("No accessToken received!");
    return res.status(400).json({ error: "No accessToken provided" });
  }
  try {
    console.log("Verifying Google Token...");
    const googleResponse = await axios.get(
      `https://www.googleapis.com/oauth2/v3/userinfo`,
      { headers: { Authorization: `Bearer ${accessToken}` } }
    );
    console.log("Google Response:", googleResponse.data);
    const { email, name } = googleResponse.data;
    const now = new Date();
    now.setHours(now.getHours() + 7); // ปรับเวลาตามไทย
    const last_active = now.toISOString().slice(0, 19).replace("T", " ");
    const role_id = 3;

    const [existingUser] = await db.query("SELECT * FROM user WHERE uid = ?", [
      email,
    ]);
    if (existingUser.length > 0) {
      await db.query(
        "UPDATE user SET last_active = ?, role_id = ? WHERE uid = ?",
        [last_active, role_id, email]
      );
      console.log(`User ${email} updated successfully`);
      notifyUnity(accessToken, email);
      return res.json({ message: "User updated successfully", userId: email });
    } else {
      await db.query(
        "INSERT INTO user (uid, name, role_id, last_active) VALUES (?, ?, ?, ?)",
        [email, name, role_id, last_active]
      );
      console.log(`User ${email} registered successfully`);
      notifyUnity(accessToken, email);
      return res.json({
        message: "User registered successfully",
        userId: email,
      });
    }
  } catch (error) {
    console.error("Google Token Verification Failed:", error);
    return res.status(400).json({ error: "Invalid Google Token" });
  }
});



app.post("/api/save", async (req, res) => {
  const saveData = req.body;
  if (!saveData) {
    return res.status(400).json({ error: "No save data provided" });
  }
  try {
    const userId = saveData.userId || "unknown";
    const practiceId = saveData.practiceId || 2;

    let score = 0;
    if (saveData.quizData && typeof saveData.quizData.score === "number") {
      score = saveData.quizData.score;
    }

    let practiceJson = "{}";
    if (
      saveData.practiceData &&
      typeof saveData.practiceData.json === "string"
    ) {
      if (saveData.practiceData.json.trim().length > 0) {
        practiceJson = saveData.practiceData.json;
      }
    }

    console.log("Saving data for user:", userId, "practiceId:", practiceId);

    const sql = `
      INSERT INTO practice_save (uid, practice_id, submit_date, score, practice_json)
      VALUES (?, ?, NOW(), ?, ?)
    `;
    const [result] = await db.query(sql, [
      userId,
      practiceId,
      score,
      practiceJson,
    ]);

    console.log(`Save data inserted with id: ${result.insertId}`);
    res.json({
      message: "Save data inserted successfully",
      id: result.insertId,
    });
  } catch (error) {
    console.error("Error saving game data:", error);
    res.status(500).json({ error: error.message });
  }
});

app.get("/api/load", async (req, res) => {
  try {
    const sql = "SELECT * FROM practice_save ORDER BY submit_date DESC LIMIT 1";
    const [rows] = await db.query(sql);
    if (rows.length === 0) {
      return res.status(404).json({ error: "No save data found" });
    }
    res.json(rows[0]);
  } catch (error) {
    console.error("Error loading game data:", error);
    res.status(500).json({ error: error.message });
  }
});

// -----------------------------------------------------------
// Endpoint สำหรับเซฟข้อมูล Simulator (INSERT)
app.post("/api/simulator/save", async (req, res) => {
  try {
    const { userId, saveJson, save_type } = req.body;
    if (!userId || !saveJson) {
      return res.status(400).json({ error: "userId or saveJson is missing" });
    }
   
    // นับจำนวน row เฉพาะ userId นี้ เพื่อจะตั้งชื่อ "Save X"
    const getCountSql =
      "SELECT COUNT(*) AS userSaves FROM SimulatorSave WHERE UID = ?";
    const [countRows] = await db.query(getCountSql, [userId]);
    const newIndex = countRows[0].userSaves + 1;

    // ตั้งชื่อเป็น Save <ลำดับของ userId นี้>
    const simulateName = `Save ${newIndex}`;

    // INSERT ลงตาราง
    const sql = `
      INSERT INTO SimulatorSave (UID, save_json, simulate_date, simulate_name ,save_type)
      VALUES (?, ?, NOW(), ? , ?)
    `;
    const [result] = await db.query(sql, [
      userId,
      saveJson,
      simulateName,
      save_type,
    ]);

    return res.json({
      message: "Data saved successfully",
      simulateName: simulateName,
      insertId: result.insertId,
    });
  } catch (error) {
    console.error("Error saving simulator data:", error);
    return res.status(500).json({ error: error.message });
  }
});

// -----------------------------------------------------------
// Endpoint สำหรับโหลดข้อมูล Simulator "ล่าสุด" (GET /api/simulator/load)
app.get("/api/simulator/load", async (req, res) => {
  try {
    const { userId } = req.query;
    if (!userId) {
      return res.status(400).json({ error: "No userId provided" });
    }

    // ดึงอันล่าสุด
    const sql = `
      SELECT * FROM SimulatorSave
      WHERE UID = ?
      ORDER BY simulate_date DESC
      LIMIT 1
    `;
    const [rows] = await db.query(sql, [userId]);

    if (!rows.length) {
      return res
        .status(404)
        .json({ error: "No save data found for this user" });
    }

    return res.json({
      message: "Load success",
      saveJson: rows[0].save_json,
      simulateName: rows[0].simulate_name, // เช่น "Save 1"
      simulateDate: rows[0].simulate_date,
    });
  } catch (error) {
    console.error("Error loading simulator data:", error);
    return res.status(500).json({ error: error.message });
  }
});

// -----------------------------------------------------------
// (ใหม่) Endpoint สำหรับ SaveDigital
app.get("/api/simulator/listSavesDigital", async (req, res) => {
  try {
    const { userId } = req.query;
    if (!userId) {
      return res.status(400).json({ error: "No userId provided" });
    }
    const sql = `
      SELECT simulate_id, simulate_name, simulate_date
      FROM SimulatorSave
      WHERE UID = ? AND save_type = 0
      ORDER BY simulate_date DESC
    `;
    const [rows] = await db.query(sql, [userId]);
    return res.json(rows);
  } catch (error) {
    console.error("Error listing simulator data:", error);
    return res.status(500).json({ error: error.message });
  }
});

// (ใหม่) Endpoint สำหรับ SaveCircuit
app.get("/api/simulator/listSavesCitcuit", async (req, res) => {
  try {
    const { userId } = req.query;
    if (!userId) {
      return res.status(400).json({ error: "No userId provided" });
    }
    const sql = `
      SELECT simulate_id, simulate_name, simulate_date
      FROM SimulatorSave
      WHERE UID = ? AND save_type = 1
      ORDER BY simulate_date DESC
    `;
    const [rows] = await db.query(sql, [userId]);
    return res.json(rows);
  } catch (error) {
    console.error("Error listing simulator data:", error);
    return res.status(500).json({ error: error.message });
  }
});

// -----------------------------------------------------------
// (ใหม่) Endpoint สำหรับ "โหลดตาม ID เฉพาะเจาะจง"
app.get("/api/simulator/loadById", async (req, res) => {
  try {
    const { userId, saveId } = req.query;
    if (!userId || !saveId) {
      return res.status(400).json({ error: "userId or saveId missing" });
    }

    const sql = `
      SELECT * FROM SimulatorSave
      WHERE UID = ? AND simulate_id = ?
      LIMIT 1
    `;
    const [rows] = await db.query(sql, [userId, saveId]);
    if (!rows.length) {
      return res.status(404).json({ error: "No save data found" });
    }

    return res.json({
      message: "Load success",
      saveJson: rows[0].save_json,
      simulateName: rows[0].simulate_name,
      simulateDate: rows[0].simulate_date,
    });
  } catch (error) {
    console.error("Error loading simulator data by id:", error);
    return res.status(500).json({ error: error.message });
  }
});
// ลบเซฟตาม userId + saveId
app.delete("/api/simulator/deleteById", async (req, res) => {
  try {
    const { userId, saveId } = req.query;
    if (!userId || !saveId) {
      return res.status(400).json({ error: "userId or saveId missing" });
    }

    // ลบ row ในตาราง SimulatorSave
    const sql = "DELETE FROM SimulatorSave WHERE UID = ? AND simulate_id = ?";
    const [result] = await db.query(sql, [userId, saveId]);

    if (result.affectedRows === 0) {
      return res
        .status(404)
        .json({
          error: "No save data found or it doesn't belong to this user",
        });
    }

    return res.json({ message: "Delete success" });
  } catch (error) {
    console.error("Error deleting simulator data:", error);
    return res.status(500).json({ error: error.message });
  }
});


 
app.get("/api/practice/find/:uid", async (req, res) => {
  const { uid } = req.params;
  try {
    const sql_find_classroom = `
      SELECT p.practice_id, p.practice_name, p.practice_detail, cp.practice_status 
      FROM enrollment AS enroll 
      JOIN classroom_practice AS cp 
      JOIN practice AS p 
        ON enroll.class_id = cp.class_id 
        AND cp.practice_id = p.practice_id 
      WHERE enroll.uid = ?
    `;
    
    const [rows] = await db.query(sql_find_classroom, [uid]);
    return res.status(200).json(rows);
  } catch (error) {
    console.error("Error selecting classroom practice data: ", error);
    return res.status(500).json({ error: error.message });
  }
});



// เริ่มเซิร์ฟเวอร์
server.listen(PORT, () => {
  console.log(`Server running on http://localhost:${PORT}`);
});
