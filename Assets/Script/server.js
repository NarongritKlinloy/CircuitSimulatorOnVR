// server.js
const express = require('express');
const app = express();
const port = 5000;

app.get('/', (req, res) => {
    res.send(`
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset="utf-8">
        <title>Google Login Callback</title>
        <script type="text/javascript">
            window.onload = function() {
                // อ่าน fragment (ส่วนหลัง #) เพื่อดึง access_token
                var hash = window.location.hash.substring(1);
                var params = new URLSearchParams(hash);
                var accessToken = params.get('access_token');
                
                if (accessToken) {
                    // เปลี่ยนไปที่ custom URL scheme เพื่อกลับเข้าเกม
                    window.location = 'mygame://login?access_token=' + accessToken;
                } else {
                    document.body.innerHTML = "<h2>ไม่พบ token ที่รับมา</h2>";
                }
            };
        </script>
    </head>
    <body>
        <h2>กำลังล็อกอิน...</h2>
    </body>
    </html>
    `);
});

app.listen(port, () => {
    console.log('Server running on port ' + port);
});
