/***************************************************
 * รวมโค้ดจากทั้งสองไฟล์เป็นไฟล์เดียว (server.js)
 ***************************************************/
require("dotenv").config();
const express = require("express");
const mysql = require("mysql2/promise");
const cors = require("cors");
const axios = require("axios");
const WebSocket = require("ws");

const app = express();
const PORT = 5000;

// 1) เปิดใช้งาน CORS, JSON Parser
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// 2) สร้าง Connection Pool (ไม่ต้องเรียก connect() เอง)
const db = mysql.createPool({
  host: "localhost",
  user: "root",
  password: "boomza532",
  database: "project_circuit",
  waitForConnections: true,
  connectionLimit: 10,
  queueLimit: 0,
});

// 3) ทดสอบเชื่อมต่อ DB (Optional)
(async function testDB() {
  try {
    const conn = await db.getConnection();
    console.log("✅ Connected to MySQL (Connection Pool)");
    conn.release();
  } catch (error) {
    console.error("❌ Cannot connect to MySQL:", error);
  }
})();

// 4) สร้าง WebSocket Server แยกพอร์ตเป็น 8080
const wss = new WebSocket.Server({ port: 8080 });
wss.on("connection", (ws) => {
  console.log("✅ Unity Connected via WebSocket");
  ws.send("🔹 Connected to WebSocket Server");
});

// ฟังก์ชันแจ้งเตือน Unity
function notifyUnity(token) {
  wss.clients.forEach((client) => {
    if (client.readyState === WebSocket.OPEN) {
      client.send(JSON.stringify({ accessToken: token }));
    }
  });
}

// -----------------------------------------------------------
// 5) Google OAuth Callback & Logout
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
              console.log("✅ Login Success:", data);

              // แจ้งเตือน Unity ผ่าน WebSocket
              fetch("http://localhost:8080/notify", {
                  method: "POST",
                  headers: { "Content-Type": "application/json" },
                  body: JSON.stringify({ accessToken: token })
              });

              // ใช้ Custom URL Scheme เพื่อส่ง Token กลับ Unity
              window.location.href = "unitydl://auth?access_token=" + token;

              // ปิด Browser
              setTimeout(() => { window.open('', '_self', ''); window.close(); }, 1000);
          })
          .catch(error => {
              console.error("❌ Error:", error);
              window.location.href = "http://localhost:5000/error";
          });
      } else {
          window.location.href = "http://localhost:5000/error";
      }
    </script>
  `);
});

app.get("/logout", (req, res) => {
  res.send(`
    <script>
      document.cookie = "G_AUTHUSER_H=; path=/; domain=google.com; expires=Thu, 01 Jan 1970 00:00:00 UTC;";
      document.cookie = "G_AUTHUSER_H=; path=/; expires=Thu, 01 Jan 1970 00:00:00 UTC;";
      window.location.href = "/";
    </script>
  `);
});

// -----------------------------------------------------------
// 6) Endpoint ลงทะเบียนผู้ใช้ (POST /register)
// -----------------------------------------------------------
app.post("/register", async (req, res) => {
  const { accessToken } = req.body;

  if (!accessToken) {
    console.error("❌ No accessToken received!");
    return res.status(400).json({ error: "❌ No accessToken provided" });
  }

  try {
    console.log("📡 Verifying Google Token...");
    const googleResponse = await axios.get(`https://www.googleapis.com/oauth2/v3/userinfo`, {
      headers: { Authorization: `Bearer ${accessToken}` },
    });

    console.log("✅ Google Response:", googleResponse.data);
    const { email, name } = googleResponse.data;
    const last_active = new Date().toISOString().slice(0, 19).replace("T", " ");
    const role_id = 3;

    // เช็คว่ามี user นี้ในระบบหรือไม่
    const [existingUser] = await db.query("SELECT * FROM user WHERE uid = ?", [email]);

    if (existingUser.length > 0) {
      // มีอยู่แล้ว -> อัปเดต
      await db.query("UPDATE user SET last_active = ?, role_id = ? WHERE uid = ?", [
        last_active,
        role_id,
        email,
      ]);
      console.log(`✅ User ${email} updated successfully`);
      notifyUnity(accessToken);
      return res.json({ message: "✅ User updated successfully" });
    } else {
      // ยังไม่มี -> สร้างใหม่
      await db.query(
        "INSERT INTO user (uid, name, role_id, last_active) VALUES (?, ?, ?, ?)",
        [email, name, role_id, last_active]
      );
      console.log(`✅ User ${email} registered successfully`);
      notifyUnity(accessToken);
      return res.json({ message: "✅ User registered successfully" });
    }
  } catch (error) {
    console.error("❌ Google Token Verification Failed:", error);
    return res.status(400).json({ error: "❌ Invalid Google Token" });
  }
});

// -----------------------------------------------------------
// 7) Endpoint /api/practice/:id (อ่าน practice_status)
// -----------------------------------------------------------
app.get("/api/practice/:id", async (req, res) => {
  const { id } = req.params;
  const sql = "SELECT practice_id, practice_status FROM practice WHERE practice_id = ?";

  try {
    const [results] = await db.query(sql, [id]);
    if (!results.length) {
      return res.status(404).json({ error: "practice_id not found" });
    }
    return res.json({
      practice_id: results[0].practice_id,
      practice_status: results[0].practice_status,
    });
  } catch (err) {
    return res.status(500).json({ error: err.message });
  }
});

// -----------------------------------------------------------
// 8) ส่วนโค้ด API ต่าง ๆ จาก snippet แรก (CRUD user, classroom, etc.)
// -----------------------------------------------------------

// ฟังก์ชันดึงข้อมูล user ตาม role
async function getUsersByRole(roleId) {
  const sql = "SELECT * FROM user WHERE role_id = ?";
  const [rows] = await db.query(sql, [roleId]);
  return rows; // ส่งกลับให้ผู้อื่นเรียกไปใช้
}

// 📚 ดึงข้อมูลนักเรียน (role_id = 3)
app.get("/api/student", async (req, res) => {
  try {
    const students = await getUsersByRole(3);
    res.status(200).json(students);
  } catch (error) {
    console.error("Database Error for role_id 3:", error);
    res.status(500).json({ error: "Database query error" });
  }
});

// 👨‍🏫 ดึงข้อมูลครู (role_id = 1)
app.get("/api/teacher", async (req, res) => {
  try {
    const teachers = await getUsersByRole(1);
    res.status(200).json(teachers);
  } catch (error) {
    console.error("Database Error for role_id 1:", error);
    res.status(500).json({ error: "Database query error" });
  }
});

// เปลี่ยน role (อัปเดต role_id ของ user)
app.put("/api/user/:uid", async (req, res) => {
  const { uid } = req.params;
  const { newrole } = req.body;

  const sql = "UPDATE user SET role_id = ? WHERE uid = ?";
  try {
    const [result] = await db.query(sql, [newrole, uid]);
    if (result.affectedRows === 0) {
      return res.status(404).json({ error: "User not found" });
    }
    res.status(200).json({ message: "Updated successfully" });
  } catch (err) {
    console.error("Error updating role:", err);
    return res.status(500).json({ error: "Update failed" });
  }
});

// ลบ user
app.delete("/api/user/:uid", async (req, res) => {
  const { uid } = req.params;
  const sql = "DELETE FROM user WHERE uid = ?";
  try {
    const [result] = await db.query(sql, [uid]);
    if (result.affectedRows === 0) {
      return res.status(404).json({ error: "User not found" });
    }
    res.status(200).json({ message: "User deleted successfully" });
  } catch (err) {
    console.error("Error deleting user:", err);
    return res.status(500).json({ error: "Delete failed" });
  }
});

// นับจำนวน user (ยกเว้น role_id = 2)
app.get("/api/user/count", async (req, res) => {
  const sql = "SELECT COUNT(*) AS userCount FROM user WHERE role_id != 2";
  try {
    const [rows] = await db.query(sql);
    const userCount = rows[0].userCount;
    res.status(200).json({ count: userCount });
  } catch (err) {
    console.error("Error counting user:", err);
    res.status(500).json({ error: "Count user failed" });
  }
});

// นับจำนวน admin (role_id = 2)
app.get("/api/admin/count", async (req, res) => {
  const sql = "SELECT COUNT(*) AS adminCount FROM user WHERE role_id = 2";
  try {
    const [rows] = await db.query(sql);
    const adminCount = rows[0].adminCount;
    res.status(200).json({ count: adminCount });
  } catch (err) {
    console.error("Error counting admin:", err);
    res.status(500).json({ error: "Count admin failed" });
  }
});

// ดึงข้อมูล practice (ทั้งหมด)
app.get("/api/practice", async (req, res) => {
  const sql = "SELECT * FROM practice";
  try {
    const [rows] = await db.query(sql);
    res.status(200).json(rows);
  } catch (err) {
    console.error("Error filtering data (practice):", err);
    res.status(500).json({ error: "Query data practice failed" });
  }
});

// เปลี่ยน status practice
app.put("/api/practice/update-status", async (req, res) => {
  const { practice_id, new_status } = req.body;
  const sql = "UPDATE practice SET practice_status = ? WHERE practice_id = ?";
  try {
    await db.query(sql, [new_status, practice_id]);
    res.status(200).send({ message: "Status updated successfully" });
  } catch (err) {
    console.error("Error updating status:", err);
    res.status(500).send("Error updating status");
  }
});

// ดึงข้อมูล classroom ทั้งหมดของครู
app.get("/api/classroom/:uid", async (req, res) => {
  const { uid } = req.params;
  const sql_teach = "SELECT class_id FROM teach WHERE uid = ?";
  try {
    const [teachRows] = await db.query(sql_teach, [uid]);
    if (teachRows.length === 0) {
      return res.status(404).json({ message: "No classrooms found for this user" });
    }
    const classIds = teachRows.map((row) => row.class_id);
    const sql_classroom = "SELECT * FROM classroom WHERE class_id IN (?)";
    const [classRows] = await db.query(sql_classroom, [classIds]);
    res.status(200).json(classRows);
  } catch (err) {
    console.error("Error filtering data (classroom):", err);
    res.status(500).json({ error: "Query data teach/classroom failed" });
  }
});

// เพิ่มข้อมูล classroom
app.post("/api/classroom", async (req, res) => {
  const { class_name, sec, semester, year, uid } = req.body;
  if (!uid) {
    return res.status(400).json({ error: "Missing 'uid' parameter" });
  }

  const sql_select_classroom =
    "SELECT * FROM classroom WHERE class_name = ? AND sec = ? AND semester = ? AND year = ?";

  try {
    const [rows] = await db.query(sql_select_classroom, [class_name, sec, semester, year]);
    if (rows.length > 0) {
      return res.status(400).json({ message: "Classroom already exists" });
    }
    // ถ้าไม่มีข้อมูลให้เพิ่ม
    const sql_insert_classroom =
      "INSERT INTO classroom (class_name, sec, semester, year) VALUES (?, ?, ?, ?)";
    const [insertResult] = await db.query(sql_insert_classroom, [
      class_name,
      sec,
      semester,
      year,
    ]);
    const class_id = insertResult.insertId;

    const sql_teach = "INSERT INTO teach (uid, class_id, role) VALUES (?, ?, 1)";
    await db.query(sql_teach, [uid, class_id]);
    res.status(200).send({ message: "Added classroom and teach successfully" });
  } catch (err) {
    console.error("Error adding classroom:", err);
    res.status(500).json({ error: "Query classroom/teach failed" });
  }
});

// ลบข้อมูล classroom
app.delete("/api/classroom/:class_id", async (req, res) => {
  const { class_id } = req.params;
  const sql_classroom = "DELETE FROM classroom WHERE class_id = ?";
  const sql_teach = "DELETE FROM teach WHERE class_id = ?";

  try {
    const [delClass] = await db.query(sql_classroom, [class_id]);
    if (delClass.affectedRows === 0) {
      return res.status(404).json({ error: "Classroom not found" });
    }
    const [delTeach] = await db.query(sql_teach, [class_id]);
    if (delTeach.affectedRows === 0) {
      return res.status(404).json({ error: "Teach not found" });
    }
    res.status(200).json({ message: "Classroom and teach deleted successfully" });
  } catch (err) {
    console.error("Error deleting classroom/teach:", err);
    res.status(500).json({ error: "Delete failed" });
  }
});

// แก้ไข classroom
app.put("/api/classroom/:id", async (req, res) => {
  try {
    const { id } = req.params;
    const { class_name, semester, sec, year } = req.body;

    if (!class_name || !semester || !sec || !year) {
      throw { status: 400, message: "Please enter data in all fields" };
    }

    // check class id
    const [checkClass] = await db.query("SELECT * FROM classroom WHERE class_id = ?", [id]);
    if (checkClass.length === 0) {
      throw { status: 404, message: "Classroom not found!" };
    }

    const sql = "UPDATE classroom SET ? WHERE class_id = ?";
    const class_data = { class_name, semester, sec, year };
    const [updateResult] = await db.query(sql, [class_data, id]);
    if (!updateResult) {
      throw { status: 400, message: "Classroom failed to update!" };
    }

    return res.status(200).json({ message: "Classroom updated successfully" });
  } catch (err) {
    const message = err.message || "Internal server error";
    const status = err.status || 500;
    return res.status(status).json({ message });
  }
});

// ดึงข้อมูลจำนวน student ที่อยู่ใน classroom
app.get("/api/classroom/student/count/:class_id", async (req, res) => {
  const { class_id } = req.params;
  const sql_enroll = "SELECT uid FROM enrollment WHERE class_id = ?";

  try {
    const [rows] = await db.query(sql_enroll, [class_id]);
    return res.status(200).json(rows.length);
  } catch (err) {
    console.error("Error select enrollment:", err);
    return res.status(500).json({ error: "Select enrollment failed" });
  }
});

// ดึงข้อมูล student ที่อยู่ใน classroom
app.get("/api/classroom/student/:class_id", async (req, res) => {
  const { class_id } = req.params;
  const sql_enroll = "SELECT uid FROM enrollment WHERE class_id = ?";

  try {
    const [rows] = await db.query(sql_enroll, [class_id]);
    if (rows.length === 0) {
      // ยังไม่มี student
      return res.status(200).json([]);
    }
    const uids = rows.map((r) => r.uid);
    const sql_user = "SELECT * FROM user WHERE uid IN (?)";
    const [userRows] = await db.query(sql_user, [uids]);
    return res.status(200).json(userRows);
  } catch (err) {
    console.error("Error select user student:", err);
    return res.status(500).json({ error: "Select user student failed" });
  }
});

// เพิ่ม student เข้า classroom
app.post("/api/classroom/student", async (req, res) => {
  const { uid, class_id } = req.body;
  if (!uid || !class_id) {
    return res.status(400).json({ error: "Missing parameter" });
  }

  // ถ้า uid ไม่ลงท้ายด้วย @kmitl.ac.th ให้เติม
  let processedUid = uid.endsWith("@kmitl.ac.th") ? uid : `${uid}@kmitl.ac.th`;

  try {
    // เช็ค user ว่ามีไหม
    const sql_user = "SELECT * FROM user WHERE uid = ?";
    const [userRows] = await db.query(sql_user, [processedUid]);
    if (userRows.length === 0) {
      return res.status(404).json({ message: "User not found" });
    }
    if (userRows[0].role_id !== 3) {
      return res.status(400).json({ message: "User is not a student" });
    }

    // เช็คว่าลงทะเบียน classroom ไปแล้วหรือยัง
    const sql_enroll_select = "SELECT * FROM enrollment WHERE uid = ?";
    const [enrollRows] = await db.query(sql_enroll_select, [processedUid]);
    if (enrollRows.length > 0) {
      return res.status(400).json({ message: "Student already has a classroom" });
    }

    // ถ้ายัง -> เพิ่ม
    const enrollDate = new Date().toISOString().slice(0, 19).replace("T", " ");
    const sql_enroll = "INSERT INTO enrollment (uid, class_id, enroll_date) VALUES (?, ?, ?)";
    await db.query(sql_enroll, [processedUid, class_id, enrollDate]);
    res.status(200).send({ message: "Added student to classroom successfully" });
  } catch (err) {
    console.error("Error insert student:", err);
    return res.status(500).json({ error: "Insert student failed" });
  }
});

// ลบ student ออกจาก classroom
app.delete("/api/classroom/student/:uid/:class_id", async (req, res) => {
  const { uid, class_id } = req.params;
  const sql_classroom = "DELETE FROM enrollment WHERE uid = ? AND class_id = ?";
  try {
    const [result] = await db.query(sql_classroom, [uid, class_id]);
    if (result.affectedRows === 0) {
      return res.status(404).json({ error: "Enrollment not found" });
    }
    res.status(200).json({ message: "Enrollment deleted successfully" });
  } catch (err) {
    console.error("Error deleting enrollment:", err);
    res.status(500).json({ error: "Delete enrollment failed" });
  }
});

// เพิ่ม user เข้าระบบ
app.post("/api/user/:uid/:name/:role_id/:last_active", async (req, res) => {
  const { uid, name, role_id, last_active } = req.params;
  const sql_select_user = "SELECT * FROM user WHERE uid = ?";
  try {
    const [rows] = await db.query(sql_select_user, [uid]);
    if (rows.length > 0) {
      // อัปเดต
      const sql_update = "UPDATE user SET name = ?, role_id = ?, last_active = ? WHERE uid = ?";
      await db.query(sql_update, [name, role_id, last_active, uid]);
    } else {
      // เพิ่มใหม่
      const sql_insert_user =
        "INSERT INTO user (uid, name, role_id, last_active) VALUES (?, ?, ?, ?)";
      await db.query(sql_insert_user, [uid, name, role_id, last_active]);
    }
    res.status(200).json({ message: "sign in successfully" });
  } catch (err) {
    console.error("Error insert/update user:", err);
    res.status(500).json({ error: "Insert/Update user failed" });
  }
});

// --------------------------- Report (Champ) ---------------------------

// ดึงข้อมูล report
app.get("/api/report", async (req, res) => {
  const { email } = req.query; // รับค่าผ่าน Query Parameters
  if (!email) {
    return res.status(400).json({ error: "Missing 'email' query parameter" });
  }
  const sql = "SELECT * FROM report WHERE report_uid = ?";
  try {
    const [rows] = await db.query(sql, [email]);
    res.status(200).json(rows);
  } catch (err) {
    console.error("Error filtering data (report):", err);
    res.status(500).json({ error: "Query data Report failed" });
  }
});

// เพิ่มข้อมูล report
app.post("/api/addreport", async (req, res) => {
  const { report_uid, report_name, report_detail, report_date } = req.body;
  if (!report_uid || !report_name || !report_detail || !report_date) {
    return res.status(400).json({ error: "กรุณากรอกข้อมูลให้ครบทุกฟิลด์" });
  }
  const parsedDate = new Date(report_date);
  if (isNaN(parsedDate.getTime())) {
    return res.status(400).json({ error: "รูปแบบวันที่ไม่ถูกต้อง" });
  }
  console.log("Request body:", req.body);
  const sql = `
    INSERT INTO report (report_uid, report_name, report_detail, report_date)
    VALUES (?, ?, ?, ?)
  `;
  try {
    const [result] = await db.query(sql, [report_uid, report_name, report_detail, parsedDate]);
    return res.status(200).json({
      message: "เพิ่มรายงานสำเร็จ",
      report_id: result.insertId,
    });
  } catch (err) {
    console.error("Error adding report:", err.message);
    return res.status(500).json({
      error: "ไม่สามารถเพิ่มข้อมูลรายงานได้",
      details: err.message,
    });
  }
});

// -----------------------------------------------------------
// 9) เริ่มต้น Server
// -----------------------------------------------------------
app.listen(PORT, () => {
  console.log(`🚀 Server running on http://localhost:${PORT}`);
});
