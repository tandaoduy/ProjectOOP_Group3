CREATE DATABASE ProjectOOP_Nhom3;
GO

USE ProjectOOP_Nhom3;
GO

-- =============================================================
-- 1. TẠO CẤU TRÚC BẢNG (DDL)
-- =============================================================

CREATE TABLE BoPhan (
    MaBP VARCHAR(10) PRIMARY KEY,
    TenBP NVARCHAR(100) NOT NULL
);

CREATE TABLE NhanVien (
    MaNV VARCHAR(20) PRIMARY KEY,
    MatKhau VARCHAR(255) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    ChucVu NVARCHAR(50), 
    TrangThai BIT DEFAULT 1
);

CREATE TABLE LoaiMatHang (
    MaLoai VARCHAR(10) PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL
);

CREATE TABLE MatHang (
    MaMH VARCHAR(10) PRIMARY KEY,
    TenMH NVARCHAR(200) NOT NULL,
    DonViTinh NVARCHAR(50),
    MaLoai VARCHAR(10),
    FOREIGN KEY (MaLoai) REFERENCES LoaiMatHang(MaLoai)
);

CREATE TABLE NhaCungCap (
    MaNCC VARCHAR(10) PRIMARY KEY,
    TenNCC NVARCHAR(200) NOT NULL,
    DiaChi NVARCHAR(MAX),
    SoDT VARCHAR(20)
);

CREATE TABLE BangGiaNhaCungCap (
    ID INT PRIMARY KEY IDENTITY(1,1),
    MaMH VARCHAR(10),
    MaNCC VARCHAR(10),
    DonGia DECIMAL(18,2) NOT NULL,
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    GhiChu NVARCHAR(255),
    FOREIGN KEY (MaMH) REFERENCES MatHang(MaMH),
    FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC)
);

CREATE TABLE PhieuYeuCau (
    MaPYC VARCHAR(20) PRIMARY KEY,
    NgayLap DATETIME DEFAULT GETDATE(),
    MaNV_Lap VARCHAR(20),
    MaBP_YeuCau VARCHAR(10),
    LyDo NVARCHAR(MAX),
    TrangThai NVARCHAR(50) DEFAULT N'Chờ duyệt',
    FOREIGN KEY (MaNV_Lap) REFERENCES NhanVien(MaNV),
    FOREIGN KEY (MaBP_YeuCau) REFERENCES BoPhan(MaBP)
);

CREATE TABLE ChiTietPYC (
    MaPYC VARCHAR(20),
    MaMH VARCHAR(10),
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    MaNCC_Gán VARCHAR(10),
    DonGiaDuKien DECIMAL(18,2),
    PRIMARY KEY (MaPYC, MaMH),
    FOREIGN KEY (MaPYC) REFERENCES PhieuYeuCau(MaPYC),
    FOREIGN KEY (MaMH) REFERENCES MatHang(MaMH),
    FOREIGN KEY (MaNCC_Gán) REFERENCES NhaCungCap(MaNCC)
);

CREATE TABLE DonDatHang (
    MaDDH VARCHAR(20) PRIMARY KEY,
    MaPYC VARCHAR(20),
    MaNCC VARCHAR(10),
    NgayLap DATETIME DEFAULT GETDATE(),
    NgayGiaoDuKien DATE,
    TongTien DECIMAL(18,2) DEFAULT 0,
    DieuKhoan NVARCHAR(MAX),
    FOREIGN KEY (MaPYC) REFERENCES PhieuYeuCau(MaPYC),
    FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC)
);

-- =============================================================
-- 2. DỮ LIỆU MẪU (ĐÃ FIX LỖI ĐỒNG BỘ ID)
-- =============================================================

-- 1. BoPhan
INSERT INTO BoPhan VALUES 
('BP01', N'Bộ phận Bếp'), ('BP02', N'Hành chính'), ('BP03', N'Kỹ thuật'), 
('BP04', N'Buồng phòng'), ('BP05', N'Nhà hàng'), ('BP06', N'Lễ tân'), 
('BP07', N'Spa'), ('BP08', N'Kinh doanh'), ('BP09', N'IT');

-- 2. NhanVien
INSERT INTO NhanVien (MaNV, MatKhau, Email, HoTen, ChucVu) VALUES 
('KT01', '123456789', 'hant@gmail.com', N'Nguyễn Thị Hà', N'Nhân viên kế toán'), 
('KT02', '123456789', 'anhtm@gmail.com', N'Trần Minh Anh', N'Nhân viên kế toán'), 
('KT03', '123456789', 'hoalt@gmail.com', N'Lê Thị Hoa', N'Nhân viên kế toán'), 
('KT04', '123456789', 'trapt@gmail.com', N'Phạm Thị Trà', N'Nhân viên kế toán'), 
('KT_TRUONG', '987654321', 'nguyetntm@gmail.com', N'Nguyễn Thị Minh Nguyệt', N'Kế toán trưởng');

-- 3. LoaiMatHang
INSERT INTO LoaiMatHang VALUES 
('L01', N'Thực phẩm Bếp'), ('L02', N'Sử dụng Nội bộ'), ('L03', N'Văn phòng phẩm'), 
('L04', N'Thiết bị điện'), ('L05', N'Hóa chất'), ('L06', N'Đồ uống');

-- 4. MatHang
INSERT INTO MatHang VALUES 
('MH01', N'Thịt heo', 'Kg', 'L01'), ('MH02', N'Giấy A4', N'Ram', 'L03'), 
('MH03', N'Nước tẩy', N'Can', 'L05'), ('MH04', N'Bóng đèn', N'Cái', 'L04'), 
('MH05', N'Dầu ăn', N'Lít', 'L01'), ('MH06', N'Gạo', 'Kg', 'L01'), 
('MH07', N'Bút bi', N'Hộp', 'L03'), ('MH08', N'Nước suối', N'Thùng', 'L06');

-- 5. NhaCungCap
INSERT INTO NhaCungCap VALUES 
('NCC01', N'Siêu thị thực phẩm', N'Đường A', '091'), ('NCC02', N'Đại lý văn phòng phẩm', N'Đường B', '092'), 
('NCC03', N'Cửa hàng điện quang', N'Đường C', '093'), ('NCC04', N'Tổng kho hóa chất', N'Đường D', '094'), 
('NCC05', N'Kho gạo miền tây', N'Đường E', '095'), ('NCC06', N'Đại lý nước giải khát', N'Đường F', '096'), 
('NCC07', N'Xưởng dệt may', N'Đường G', '097'), ('NCC08', N'Cửa hàng kim khí', N'Đường H', '098'), 
('NCC09', N'Bảo hộ lao động X', N'Đường I', '099'), ('NCC10', N'Tạp hóa tổng hợp', N'Đường J', '090');

-- 6. BangGiaNhaCungCap (Sửa MH10 thành MH08 vì bảng MatHang chỉ có đến MH08)
INSERT INTO BangGiaNhaCungCap (MaMH, MaNCC, DonGia, GhiChu) VALUES 
('MH01', 'NCC01', 125000, N'Giá thịt heo tươi loại 1'),
('MH01', 'NCC10', 130000, N'Giá thịt heo tại siêu thị'),
('MH02', 'NCC02', 62000, N'Giấy A4 Double A'),
('MH02', 'NCC10', 65000, N'Giấy A4 thông dụng'),
('MH05', 'NCC01', 42000, N'Dầu ăn Tường An 1L'),
('MH05', 'NCC10', 38000, N'Dầu ăn Simply 1L (Khuyến mãi)'),
('MH06', 'NCC05', 18000, N'Gạo thơm lài'),
('MH06', 'NCC10', 21000, N'Gạo đóng túi siêu thị'),
('MH08', 'NCC06', 85000, N'Nước suối Aquafina thùng 24 chai'),
('MH08', 'NCC10', 92000, N'Nước suối La Vie thùng 24 chai');

-- 7. PhieuYeuCau 
INSERT INTO PhieuYeuCau (MaPYC, MaNV_Lap, MaBP_YeuCau, LyDo, TrangThai) VALUES 
('PYC01', 'KT01', 'BP01', N'Nấu ăn sáng', N'Đã duyệt'), 
('PYC02', 'KT02', 'BP02', N'In ấn hồ sơ', N'Chờ duyệt'),  
('PYC03', 'KT01', 'BP01', N'Dự trữ kho bếp', N'Đã duyệt'), 
('PYC04', 'KT03', 'BP05', N'Phục vụ tiệc', N'Đã duyệt'), 
('PYC05', 'KT02', 'BP02', N'Văn phòng phẩm quý', N'Đã duyệt'), 
('PYC06', 'KT04', 'BP09', N'Thiết bị mạng', N'Chờ duyệt'), 
('PYC07', 'KT01', 'BP01', N'Gia vị bếp', N'Đã duyệt');

-- 8. ChiTietPYC 
INSERT INTO ChiTietPYC VALUES ('PYC01', 'MH01', 20, 'NCC01', 120000);

-- 9. DonDatHang 
INSERT INTO DonDatHang (MaDDH, MaPYC, MaNCC, NgayGiaoDuKien, TongTien) VALUES 
('DDH01', 'PYC01', 'NCC01', '2025-12-21', 2400000);

ALTER TABLE PhieuYeuCau
ADD MaNV_Duyet VARCHAR(20),
    NgayDuyet DATETIME,
    GhiChuDuyet NVARCHAR(MAX);

ALTER TABLE PhieuYeuCau
ADD CONSTRAINT FK_NhanVien_Duyet FOREIGN KEY (MaNV_Duyet) REFERENCES NhanVien(MaNV);