<!DOCTYPE html>
<html lang="vi">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Coffee House - Tổng quan</title>
<style>
  :root{
    --brown-950:#1f1109;
    --brown-900:#2a170d;
    --brown-800:#341c10;
    --cream:#f5f2eb;
    --card:#ffffff;
    --ink:#241a12;
    --muted:#948879;
    --line:#ece6db;
    --accent-orange:#c8763c;
    --accent-green:#3f9a4a;
    --accent-yellow:#e0a730;
    --accent-red:#d6564a;
    --radius:14px;
  }
  *{box-sizing:border-box;}
  body{
    margin:0;
    font-family:'Segoe UI','Helvetica Neue',Arial,sans-serif;
    background:var(--cream);
    color:var(--ink);
    font-size:14px;
  }
  .app{display:flex;min-height:100vh;}

  /* ================= SIDEBAR ================= */
  .sidebar{
    width:230px;flex-shrink:0;
    background:linear-gradient(180deg,var(--brown-950),var(--brown-900) 55%,var(--brown-800));
    color:#e9dfd0;
    display:flex;flex-direction:column;
    padding:20px 14px;
  }
  .brand{
    display:flex;align-items:center;gap:10px;
    padding:6px 6px 20px 6px;
    border-bottom:1px solid rgba(255,255,255,0.08);
    margin-bottom:14px;
  }
  /* ---- LOGO PLACEHOLDER: thay src bên dưới bằng logo thật ---- */
  .logo-placeholder{
    width:40px;height:40px;border-radius:10px;flex-shrink:0;
    object-fit:cover;
    border:1.5px dashed rgba(255,255,255,0.35);
    background:rgba(255,255,255,0.06);
  }
  .brand-text b{display:block;font-size:15px;letter-spacing:.2px;}
  .brand-text span{display:block;font-size:11px;color:#c3b6a2;margin-top:1px;}
  nav{flex:1;display:flex;flex-direction:column;gap:3px;margin-top:6px;}
  .nav-item{
    display:flex;align-items:center;gap:12px;
    padding:10px 12px;border-radius:10px;
    color:#d6c8b5;text-decoration:none;font-size:13.5px;
    cursor:pointer;
  }
  .nav-item .ic{width:18px;text-align:center;font-size:14px;}
  .nav-item:hover{background:rgba(255,255,255,0.06);}
  .nav-item.active{background:var(--accent-orange);color:#fff;font-weight:600;}
  .branch-switch{margin-top:12px;padding-top:14px;border-top:1px solid rgba(255,255,255,0.08);}
  .branch-switch label{font-size:10.5px;color:#a99b86;display:block;margin-bottom:6px;letter-spacing:.3px;}
  .branch-select{
    display:flex;align-items:center;justify-content:space-between;
    background:rgba(255,255,255,0.07);
    padding:9px 10px;border-radius:10px;font-size:12.5px;color:#f1e9dd;
  }

  /* ================= MAIN ================= */
  .main{flex:1;min-width:0;display:flex;flex-direction:column;}
  .topbar{
    display:flex;align-items:center;justify-content:space-between;
    padding:16px 26px;background:#fff;border-bottom:1px solid var(--line);
  }
  .topbar-left{display:flex;align-items:center;gap:14px;font-size:16px;font-weight:700;}
  .burger{font-size:18px;color:#5c5346;cursor:pointer;}
  .topbar-right{display:flex;align-items:center;gap:16px;}
  .date-range{
    display:flex;align-items:center;gap:8px;border:1px solid var(--line);
    padding:8px 14px;border-radius:10px;font-size:12.5px;color:#5c5346;
  }
  .bell{
    width:36px;height:36px;border-radius:50%;
    display:flex;align-items:center;justify-content:center;
    background:var(--cream);font-size:15px;cursor:pointer;
  }
  .avatar{width:36px;height:36px;border-radius:50%;object-fit:cover;}

  .content{padding:22px 26px 40px;display:flex;flex-direction:column;gap:20px;}

  /* ---- Stat cards ---- */
  .stat-grid{display:grid;grid-template-columns:repeat(3,1fr);gap:18px;}
  .stat-card{background:var(--card);border-radius:var(--radius);padding:20px 20px 14px;box-shadow:0 1px 4px rgba(30,20,10,0.07);}
  .stat-label{font-size:12.5px;color:var(--muted);text-align:center;margin-bottom:6px;}
  .stat-value{font-size:26px;font-weight:800;text-align:center;margin-bottom:8px;}
  .stat-change{display:flex;align-items:center;justify-content:center;gap:5px;font-size:12px;font-weight:600;margin-bottom:10px;color:var(--accent-green);}

  /* ---- CHART PLACEHOLDER (dùng chung cho sparkline / bar / donut) ---- */
  .chart-placeholder{
    display:block;width:100%;
    border:1.5px dashed #d9cdb8;border-radius:10px;
    object-fit:cover;background:#faf7f1;
  }
  .sparkline-img{height:64px;}
  .bar-chart-img{height:230px;}
  .donut-img{width:150px;height:150px;border-radius:50%;flex-shrink:0;}

  /* ---- Generic card ---- */
  .card{background:var(--card);border-radius:var(--radius);padding:18px 20px 20px;box-shadow:0 1px 4px rgba(30,20,10,0.07);}
  .card-head{display:flex;align-items:center;justify-content:space-between;margin-bottom:14px;}
  .card-title{font-size:14.5px;font-weight:700;}
  .card-sub{font-size:11.5px;color:var(--muted);border:1px solid var(--line);padding:5px 10px;border-radius:8px;display:flex;align-items:center;gap:5px;}
  .card-link{font-size:12px;color:var(--accent-orange);font-weight:600;cursor:pointer;}

  .row-2{display:grid;grid-template-columns:1.65fr 1fr;gap:18px;align-items:stretch;}
  .row-2.b{grid-template-columns:1fr 1.65fr;}

  /* ---- Product list ---- */
  .product-list{display:flex;flex-direction:column;gap:14px;}
  .product-row{display:flex;align-items:center;gap:12px;}
  .product-icon{width:38px;height:38px;border-radius:10px;flex-shrink:0;object-fit:cover;}
  .product-name{font-size:13px;font-weight:600;}
  .product-qty{font-size:11.5px;color:var(--muted);}
  .product-rev{margin-left:auto;font-size:13px;font-weight:700;white-space:nowrap;}

  /* ---- Donut + legend ---- */
  .donut-wrap{display:flex;align-items:center;gap:22px;}
  .legend{display:flex;flex-direction:column;gap:14px;}
  .legend-item{display:flex;align-items:center;gap:8px;font-size:12.5px;}
  .dot{width:9px;height:9px;border-radius:50%;flex-shrink:0;}
  .legend-count{color:var(--muted);margin-left:2px;}

  /* ---- Orders table ---- */
  table{width:100%;border-collapse:collapse;font-size:12.5px;}
  th{text-align:left;color:var(--muted);font-weight:600;font-size:11.5px;padding:0 10px 10px 0;text-transform:uppercase;letter-spacing:.2px;}
  td{padding:11px 10px 11px 0;border-top:1px solid var(--line);}
  .badge{display:inline-block;padding:4px 10px;border-radius:20px;font-size:11px;font-weight:700;}
  .badge.completed{background:#dff2e1;color:var(--accent-green);}
  .badge.preparing{background:#fbeed3;color:var(--accent-yellow);}
  .badge.cancelled{background:#f9dedb;color:var(--accent-red);}

  @media (max-width:980px){
    .sidebar{display:none;}
    .stat-grid{grid-template-columns:1fr;}
    .row-2,.row-2.b{grid-template-columns:1fr;}
  }
</style>
</head>
<body>
<div class="app">

  <!-- ============ SIDEBAR ============ -->
  <aside class="sidebar">
    <div class="brand">
      <!-- LOGO: thay thuộc tính src bằng đường dẫn ảnh logo của bạn -->
      <img class="logo-placeholder" src="" alt="Logo Coffee House">
      <div class="brand-text">
        <b>Coffee House</b>
        <span>Quản lý quán cà phê</span>
      </div>
    </div>
    <nav>
      <div class="nav-item active"><span class="ic">🏠</span> Tổng quan</div>
      <div class="nav-item"><span class="ic">🧾</span> Đơn hàng</div>
      <div class="nav-item"><span class="ic">☕</span> Sản phẩm</div>
      <div class="nav-item"><span class="ic">📦</span> Kho hàng</div>
      <div class="nav-item"><span class="ic">👤</span> Khách hàng</div>
      <div class="nav-item"><span class="ic">👥</span> Nhân viên</div>
      <div class="nav-item"><span class="ic">🕒</span> Ca làm việc</div>
      <div class="nav-item"><span class="ic">💵</span> Chi tiêu</div>
      <div class="nav-item"><span class="ic">📊</span> Báo cáo</div>
      <div class="nav-item"><span class="ic">🏷️</span> Khuyến mãi</div>
    </nav>
    <div class="branch-switch">
      <label>CHI NHÁNH HIỆN TẠI</label>
      <div class="branch-select"><span>Coffee House - Cầu Giấy</span><span>⌄</span></div>
    </div>
  </aside>

  <!-- ============ MAIN ============ -->
  <div class="main">
    <div class="topbar">
      <div class="topbar-left"><span class="burger">☰</span> Tổng quan</div>
      <div class="topbar-right">
        <div class="date-range">📅 10/07/2026 - 15/7/2026 ⌄</div>
        <div class="bell">🔔</div>
        <img class="avatar" src="https://i.pravatar.cc/72" alt="Avatar">
        <span>⌄</span>
      </div>
    </div>

    <div class="content">

      <!-- ===== STAT CARDS ===== -->
      <div class="stat-grid">
        <div class="stat-card">
          <div class="stat-label">Doanh thu</div>
          <div class="stat-value">24.590.000đ</div>
          <div class="stat-change">↑ 12,5% so với tuần trước</div>
          <img class="chart-placeholder sparkline-img" src="" alt="Biểu đồ xu hướng doanh thu">
        </div>
        <div class="stat-card">
          <div class="stat-label">Đơn hàng</div>
          <div class="stat-value">320</div>
          <div class="stat-change">↑ 12,5% so với tuần trước</div>
          <img class="chart-placeholder sparkline-img" src="" alt="Biểu đồ xu hướng đơn hàng">
        </div>
        <div class="stat-card">
          <div class="stat-label">Khách hàng</div>
          <div class="stat-value">256</div>
          <div class="stat-change">↑ 12,5% so với tuần trước</div>
          <img class="chart-placeholder sparkline-img" src="" alt="Biểu đồ xu hướng khách hàng">
        </div>
      </div>

      <!-- ===== REVENUE CHART + TOP PRODUCTS ===== -->
      <div class="row-2">
        <div class="card">
          <div class="card-head">
            <div class="card-title">Doanh thu theo ngày</div>
            <div class="card-sub">7 ngày qua ⌄</div>
          </div>
          <!-- BIỂU ĐỒ CỘT: thay src bằng ảnh biểu đồ doanh thu theo ngày -->
          <img class="chart-placeholder bar-chart-img" src="" alt="Biểu đồ doanh thu theo ngày (10/05 - 16/05)">
        </div>
        <div class="card">
          <div class="card-head">
            <div class="card-title">Sản phẩm bán chạy</div>
            <div class="card-link">Xem tất cả</div>
          </div>
          <div class="product-list">
            <div class="product-row">
              <img class="product-icon" src="" alt="Cà phê đen đá">
              <div>
                <div class="product-name">Cà phê đen đá</div>
                <div class="product-qty">120 lượt</div>
              </div>
              <div class="product-rev">1.800.000 đ</div>
            </div>
            <div class="product-row">
              <img class="product-icon" src="" alt="Cà phê sữa đá">
              <div>
                <div class="product-name">Cà phê sữa đá</div>
                <div class="product-qty">98 lượt</div>
              </div>
              <div class="product-rev">1.764.000 đ</div>
            </div>
            <div class="product-row">
              <img class="product-icon" src="" alt="Bạc xỉu">
              <div>
                <div class="product-name">Bạc xỉu</div>
                <div class="product-qty">76 lượt</div>
              </div>
              <div class="product-rev">1.520.000 đ</div>
            </div>
            <div class="product-row">
              <img class="product-icon" src="" alt="Trà đào cam sả">
              <div>
                <div class="product-name">Trà đào cam sả</div>
                <div class="product-qty">65 lượt</div>
              </div>
              <div class="product-rev">1.300.000 đ</div>
            </div>
            <div class="product-row">
              <img class="product-icon" src="" alt="Matcha đá xay">
              <div>
                <div class="product-name">Matcha đá xay</div>
                <div class="product-qty">54 lượt</div>
              </div>
              <div class="product-rev">1.512.000 đ</div>
            </div>
          </div>
        </div>
      </div>

      <!-- ===== ORDER STATUS DONUT + RECENT ORDERS ===== -->
      <div class="row-2 b">
        <div class="card">
          <div class="card-head">
            <div class="card-title">Đơn hàng</div>
            <div class="card-sub">7 ngày qua ⌄</div>
          </div>
          <div class="donut-wrap">
            <!-- BIỂU ĐỒ TRÒN: thay src bằng ảnh biểu đồ donut (giữa hiển thị 320 / Tổng đơn) -->
            <img class="chart-placeholder donut-img" src="" alt="Biểu đồ tỉ lệ trạng thái đơn hàng - Tổng 320 đơn">
            <div class="legend">
              <div class="legend-item"><span class="dot" style="background:var(--accent-green)"></span>Đã hoàn thành <span class="legend-count">256 (80%)</span></div>
              <div class="legend-item"><span class="dot" style="background:var(--accent-yellow)"></span>Đang xử lý <span class="legend-count">40 (12.5%)</span></div>
              <div class="legend-item"><span class="dot" style="background:var(--accent-red)"></span>Đã hủy <span class="legend-count">24 (7.5%)</span></div>
            </div>
          </div>
        </div>
        <div class="card">
          <div class="card-head">
            <div class="card-title">Đơn hàng gần đây</div>
            <div class="card-link">Xem tất cả</div>
          </div>
          <table>
            <thead>
              <tr><th>Mã đơn</th><th>Khách hàng</th><th>Sản phẩm</th><th>Tổng tiền</th><th>Trạng thái</th></tr>
            </thead>
            <tbody>
              <tr>
                <td>#DH10045</td><td>Nguyễn Minh Anh</td><td>3 sản phẩm</td><td>135.000đ</td>
                <td><span class="badge completed">Hoàn thành</span></td>
              </tr>
              <tr>
                <td>#DH10044</td><td>Trần Quang Huy</td><td>2 sản phẩm</td><td>90.000đ</td>
                <td><span class="badge preparing">Đang xử lý</span></td>
              </tr>
              <tr>
                <td>#DH10043</td><td>Lê Thị Mai</td><td>4 sản phẩm</td><td>185.000đ</td>
                <td><span class="badge completed">Hoàn thành</span></td>
              </tr>
              <tr>
                <td>#DH10042</td><td>Phạm Hoàng Nam</td><td>1 sản phẩm</td><td>45.000đ</td>
                <td><span class="badge cancelled">Đã hủy</span></td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  </div>
</div>
</body>
</html>