const express = require("express");
const mysql = require("mysql2");
const bodyParser = require("body-parser");
const { OAuth2Client } = require("google-auth-library");

const app = express();
const port = 5000;

// ตั้งค่า MySQL
const db = mysql.createConnection({
  host: "localhost",
  user: "root",
  password: "boomza532", // ใส่รหัสผ่าน MySQL ของคุณ
  database: "project_circuit",
});

db.connect((err) => {
  if (err) {
    console.error("Error connecting to database:", err.stack);
    return;
  }
  console.log("Connected to database");
});

app.use(bodyParser.urlencoded({ extended: true }));
app.use(bodyParser.json());

// ตั้งค่า Google OAuth Client
const CLIENT_ID = "536241701089-qploj9k3osp4oel3gcu4eq0c9eqi16rm.apps.googleusercontent.com";
const client = new OAuth2Client(CLIENT_ID);

// API สำหรับตรวจสอบ Google Sign-In และบันทึกลงใน MySQL
app.post("/register", (req, res) => {
    const { uid, name } = req.body;
  
    if (!uid || !name) {
      return res.status(400).json({ error: "Invalid or missing data" });
    }
  
    // ตรวจสอบรูปแบบอีเมลที่ต้องการ: 8 ตัวเลขตามด้วย @kmitl.ac.th
    const emailRegex = /^[0-9]{8}@kmitl\.ac\.th$/;
    
    if (!emailRegex.test(uid)) {
      return res.status(400).json({ error: "Invalid email format. Please use 8-digit numbers followed by @kmitl.ac.th" });
    }
  
    const defaultRoleId = 3; // กำหนด role_id ให้เป็น 3 เสมอ
  
    const query = `
      INSERT INTO users (uid, name, role_id, last_active)
      VALUES (?, ?, ?, NOW())
      ON DUPLICATE KEY UPDATE 
        name = VALUES(name),
        last_active = NOW()
    `;
  
    db.query(query, [uid, name, defaultRoleId], (err, result) => {
      if (err) {
        console.error("Error inserting user:", err);
        return res.status(500).json({ error: "Failed to register user" });
      }
      res.status(200).json({ message: "User registered successfully!" });
    });
  });
  
  

// เริ่มต้นเซิร์ฟเวอร์
app.listen(port, () => {
  console.log(`Server running at http://localhost:${port}`);
});
