require("dotenv").config();
const express = require("express");
const mysql = require("mysql2/promise");
const axios = require("axios");
const WebSocket = require("ws");

const app = express();
const PORT = 3000;

// ✅ WebSocket Server
const wss = new WebSocket.Server({ port: 8080 });

wss.on("connection", (ws) => {
    console.log("✅ Unity Connected via WebSocket");
    ws.send("🔹 Connected to WebSocket Server");
});

// ✅ ฟังก์ชันแจ้งเตือน Unity
function notifyUnity(token) {
    wss.clients.forEach(client => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(JSON.stringify({ accessToken: token }));
        }
    });
}

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// ✅ ตั้งค่า MySQL Connection Pool
const db = mysql.createPool({
    host: "localhost",
    user: "root",
    password: "boomza532",  // เปลี่ยนเป็นของคุณ
    database: "project_circuit",
    waitForConnections: true,
    connectionLimit: 10,
    queueLimit: 0,
});

// ✅ Google OAuth Callback
app.get("/callback", (req, res) => {
    res.send(`<script>
        const hash = window.location.hash.substring(1);
        const params = new URLSearchParams(hash);
        const token = params.get("access_token");

        if (token) {
            fetch("http://localhost:3000/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ accessToken: token })
            })
            .then(response => response.json())
            .then(data => {
                console.log("✅ Login Success:", data);

                // ✅ แจ้งเตือน Unity ผ่าน WebSocket
                fetch("http://localhost:8080/notify", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ accessToken: token })
                });

                // ✅ ใช้ Custom URL Scheme เพื่อส่ง Token กลับ Unity
                window.location.href = "unitydl://auth?access_token=" + token;

                // ✅ ปิด Browser
                setTimeout(() => { window.open('', '_self', ''); window.close(); }, 1000);
            })
            .catch(error => {
                console.error("❌ Error:", error);
                window.location.href = "http://localhost:3000/error";
            });
        } else {
            window.location.href = "http://localhost:3000/error";
        }
    </script>`);
});

app.get("/logout", (req, res) => {
    res.send(`<script>
        document.cookie = "G_AUTHUSER_H=; path=/; domain=google.com; expires=Thu, 01 Jan 1970 00:00:00 UTC;";
        document.cookie = "G_AUTHUSER_H=; path=/; expires=Thu, 01 Jan 1970 00:00:00 UTC;";
        window.location.href = "/";
    </script>`);
});

// ✅ Endpoint ลงทะเบียนผู้ใช้
app.post("/register", async (req, res) => {
    const { accessToken } = req.body;

    if (!accessToken) {
        console.error("❌ No accessToken received!");
        return res.status(400).json({ error: "❌ No accessToken provided" });
    }

    try {
        console.log("📡 Verifying Google Token...");
        const googleResponse = await axios.get(`https://www.googleapis.com/oauth2/v3/userinfo`, {
            headers: { Authorization: `Bearer ${accessToken}` }
        });

        console.log("✅ Google Response:", googleResponse.data);
        const { email, name } = googleResponse.data;
        const last_active = new Date().toISOString().slice(0, 19).replace("T", " ");
        const role_id = 3;

        const [existingUser] = await db.query("SELECT * FROM user WHERE uid = ?", [email]);

        if (existingUser.length > 0) {
            await db.query("UPDATE user SET last_active = ?, role_id = ? WHERE uid = ?", [last_active, role_id, email]);
            console.log(`✅ User ${email} updated successfully`);
            notifyUnity(accessToken); // แจ้ง Unity
            return res.json({ message: "✅ User updated successfully" });
        } else {
            await db.query("INSERT INTO user (uid, name, role_id, last_active) VALUES (?, ?, ?, ?)", 
                [email, name, role_id, last_active]);
            console.log(`✅ User ${email} registered successfully`);
            notifyUnity(accessToken); // แจ้ง Unity
            return res.json({ message: "✅ User registered successfully" });
        }
    } catch (error) {
        console.error("❌ Google Token Verification Failed:", error);
        res.status(400).json({ error: "❌ Invalid Google Token" });
    }
});

app.listen(PORT, () => console.log(`🚀 Server running on http://localhost:${PORT}`));
