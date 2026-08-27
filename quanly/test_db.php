<?php

require_once "config/database.php";

echo "<h3>Kết nối MySQL thành công!</h3>";

try {
    $stmt = $conn->query("SHOW TABLES");

    echo "<h4>Danh sách bảng:</h4>";

    while ($row = $stmt->fetch(PDO::FETCH_NUM)) {
        echo $row[0] . "<br>";
    }

} catch (PDOException $e) {
    echo "Lỗi: " . $e->getMessage();
}