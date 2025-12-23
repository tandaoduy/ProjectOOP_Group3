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

-- 2. DỮ LIỆU MẪU 

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
('MH01', N'Thịt heo', N'Kg', 'L01'),
('MH02', N'Thịt bò', N'Kg', 'L01'),
('MH03', N'Gà ta', N'Kg', 'L01'),
('MH04', N'Cá basa', N'Kg', 'L01'),
('MH05', N'Dầu ăn', N'Lít', 'L01'),
('MH06', N'Gạo thơm', N'Kg', 'L01'),
('MH07', N'Muối tinh', N'Kg', 'L01'),
('MH08', N'Đường cát', N'Kg', 'L01'),
('MH09', N'Nước suối', N'Thùng', 'L06'),
('MH10', N'Nước ngọt Coca', N'Thùng', 'L06'),

('MH11', N'Giấy A4', N'Ram', 'L03'),
('MH12', N'Bút bi', N'Hộp', 'L03'),
('MH13', N'Sổ tay', N'Cuốn', 'L03'),
('MH14', N'Bìa hồ sơ', N'Cái', 'L03'),
('MH15', N'Kẹp giấy', N'Hộp', 'L03'),

('MH16', N'Bóng đèn LED', N'Cái', 'L04'),
('MH17', N'Ổ cắm điện', N'Cái', 'L04'),
('MH18', N'Dây điện', N'Mét', 'L04'),
('MH19', N'Quạt thông gió', N'Cái', 'L04'),
('MH20', N'Công tắc điện', N'Cái', 'L04'),

('MH21', N'Nước lau sàn', N'Can', 'L05'),
('MH22', N'Nước rửa chén', N'Can', 'L05'),
('MH23', N'Nước tẩy toilet', N'Can', 'L05'),
('MH24', N'Khăn lau', N'Bịch', 'L02'),
('MH25', N'Bao rác', N'Cuộn', 'L02'),

('MH26', N'Áo bảo hộ', N'Bộ', 'L02'),
('MH27', N'Găng tay cao su', N'Hộp', 'L02'),
('MH28', N'Mũ bảo hộ', N'Cái', 'L02'),
('MH29', N'Giày bảo hộ', N'Đôi', 'L02'),
('MH30', N'Khẩu trang y tế', N'Hộp', 'L02');

-- 5. NhaCungCap
INSERT INTO NhaCungCap VALUES
('NCC01', N'Công ty CP Thực phẩm Vissan', N'TP.HCM', '02838670000'),
('NCC02', N'Công ty CP Việt Nam Kỹ Nghệ Súc Sản', N'TP.HCM', '02838561111'),
('NCC03', N'Công ty Lương thực Miền Nam', N'TP.HCM', '02838293333'),
('NCC04', N'Công ty CP Acecook Việt Nam', N'TP.HCM', '02837541111'),
('NCC05', N'Công ty TNHH Dầu thực vật Tường An', N'TP.HCM', '02838470000'),

('NCC06', N'Công ty TNHH Nestlé Việt Nam', N'Đồng Nai', '02513820000'),
('NCC07', N'Công ty Coca-Cola Việt Nam', N'TP.HCM', '02838229999'),
('NCC08', N'Công ty Suntory PepsiCo VN', N'TP.HCM', '02854160000'),

('NCC09', N'Công ty CP Văn phòng phẩm Hồng Hà', N'Hà Nội', '02439433333'),
('NCC10', N'Công ty CP Văn phòng phẩm Thiên Long', N'TP.HCM', '02837505050'),

('NCC11', N'Công ty Điện Quang', N'TP.HCM', '02838683333'),
('NCC12', N'Công ty Rạng Đông', N'Hà Nội', '02438583333'),
('NCC13', N'Công ty Panasonic Việt Nam', N'Hà Nội', '02439420000'),

('NCC14', N'Công ty CP Hóa chất Đức Giang', N'Hà Nội', '02438330666'),
('NCC15', N'Công ty Unilever Việt Nam', N'TP.HCM', '02838245000'),

('NCC16', N'Công ty Bảo hộ lao động Thăng Long', N'Hà Nội', '02435670000'),
('NCC17', N'Công ty CP May Việt Tiến', N'TP.HCM', '02838640000'),
('NCC18', N'Công ty CP May Nhà Bè', N'TP.HCM', '02838730000'),

('NCC19', N'Công ty CP Kim khí Thăng Long', N'Hà Nội', '02439410000'),
('NCC20', N'Công ty CP Cơ điện Trần Phú', N'Hải Phòng', '02253860000'),

('NCC21', N'Siêu thị Metro Cash & Carry', N'TP.HCM', '02862800000'),
('NCC22', N'Siêu thị Big C Việt Nam', N'TP.HCM', '02862850000'),
('NCC23', N'Siêu thị Coopmart', N'TP.HCM', '02839305050'),

('NCC24', N'Công ty Lavie', N'Long An', '02723500000'),
('NCC25', N'Công ty Aquafina Việt Nam', N'TP.HCM', '02838229999'),

('NCC26', N'Công ty CP Bao bì Tân Tiến', N'Bình Dương', '02743780000'),
('NCC27', N'Công ty CP Nhựa Bình Minh', N'TP.HCM', '02839650000'),
('NCC28', N'Công ty CP Nhựa Tiền Phong', N'Hải Phòng', '02253850000'),
('NCC29', N'Công ty CP Hóa mỹ phẩm Mỹ Hảo', N'Long An', '02723820000'),
('NCC30', N'Công ty CP Dệt May Thành Công', N'TP.HCM', '02838680000');


-- 6. BangGiaNhaCungCap (Sửa MH10 thành MH08 vì bảng MatHang chỉ có đến MH08)
INSERT INTO BangGiaNhaCungCap (MaMH, MaNCC, DonGia, GhiChu) VALUES
('MH01','NCC01',120000,N'Thịt heo loại 1'),
('MH02','NCC01',220000,N'Thịt bò Úc'),
('MH06','NCC03',18000,N'Gạo thơm ST25'),
('MH05','NCC05',42000,N'Dầu ăn Tường An'),
('MH09','NCC24',85000,N'Nước Lavie'),

('MH10','NCC07',210000,N'Coca-Cola thùng'),
('MH11','NCC09',62000,N'Giấy A4'),
('MH12','NCC10',95000,N'Bút bi TL'),
('MH16','NCC11',48000,N'Bóng đèn LED'),
('MH17','NCC13',65000,N'Ổ cắm Panasonic'),

('MH21','NCC15',110000,N'Nước lau sàn'),
('MH22','NCC15',98000,N'Nước rửa chén'),
('MH23','NCC14',75000,N'Nước tẩy'),
('MH26','NCC16',320000,N'Áo bảo hộ'),
('MH27','NCC16',85000,N'Găng tay'),

('MH28','NCC16',120000,N'Mũ bảo hộ'),
('MH29','NCC16',450000,N'Giày bảo hộ'),
('MH30','NCC29',68000,N'Khẩu trang'),

('MH13','NCC09',15000,N'Sổ tay'),
('MH14','NCC09',12000,N'Bìa hồ sơ'),
('MH15','NCC09',18000,N'Kẹp giấy'),
('MH18','NCC20',12000,N'Dây điện'),
('MH19','NCC20',850000,N'Quạt hút'),
('MH20','NCC20',25000,N'Công tắc'),

('MH24','NCC26',32000,N'Khăn lau'),
('MH25','NCC26',45000,N'Bao rác'),
('MH07','NCC21',8000,N'Muối'),
('MH08','NCC21',19000,N'Đường'),
('MH04','NCC02',65000,N'Cá basa');


-- 7. PhieuYeuCau 
INSERT INTO PhieuYeuCau (MaPYC, MaNV_Lap, MaBP_YeuCau, LyDo, TrangThai) VALUES
('PYC01','KT01','BP01',N'Nấu ăn sáng',N'Đã duyệt'),
('PYC02','KT02','BP02',N'In ấn hồ sơ',N'Chờ duyệt'),
('PYC03','KT01','BP01',N'Dự trữ kho bếp',N'Đã duyệt'),
('PYC04','KT03','BP05',N'Phục vụ tiệc',N'Đã duyệt'),
('PYC05','KT02','BP02',N'Văn phòng phẩm quý',N'Đã duyệt'),
('PYC06','KT04','BP09',N'Thiết bị mạng',N'Chờ duyệt'),
('PYC07','KT01','BP01',N'Gia vị bếp',N'Đã duyệt'),
('PYC08','KT03','BP04',N'Vật dụng buồng phòng',N'Đã duyệt'),
('PYC09','KT02','BP06',N'Chuẩn bị lễ tân',N'Đã duyệt'),
('PYC10','KT04','BP07',N'Spa tiêu hao vật tư',N'Chờ duyệt');

-- 8. ChiTietPYC 
INSERT INTO ChiTietPYC (MaPYC, MaMH, SoLuong, MaNCC_Gán, DonGiaDuKien) VALUES
('PYC01','MH01',20,'NCC01',125000),
('PYC02','MH02',10,'NCC02',62000),
('PYC03','MH06',50,'NCC05',18000),
('PYC04','MH08',15,'NCC06',85000),
('PYC05','MH07',5,'NCC02',45000),
('PYC06','MH04',8,'NCC03',90000),
('PYC07','MH05',12,'NCC01',42000),
('PYC08','MH03',6,'NCC04',110000),
('PYC09','MH02',7,'NCC10',65000),
('PYC10','MH03',4,'NCC04',115000);

-- 9. DonDatHang 
INSERT INTO DonDatHang (MaDDH, MaPYC, MaNCC, NgayGiaoDuKien, TongTien) VALUES
('DDH01','PYC01','NCC01','2025-12-05',2500000),
('DDH02','PYC02','NCC02','2025-12-06',620000),
('DDH03','PYC03','NCC05','2025-12-07',900000),
('DDH04','PYC04','NCC06','2025-12-08',1275000),
('DDH05','PYC05','NCC02','2025-12-09',225000),
('DDH06','PYC06','NCC03','2025-12-10',720000),
('DDH07','PYC07','NCC01','2025-12-11',504000),
('DDH08','PYC08','NCC04','2025-12-12',660000),
('DDH09','PYC09','NCC10','2025-12-13',455000),
('DDH10','PYC10','NCC04','2025-12-14',460000);

ALTER TABLE PhieuYeuCau
ADD MaNV_Duyet VARCHAR(20),
    NgayDuyet DATETIME,
    GhiChuDuyet NVARCHAR(MAX);

ALTER TABLE PhieuYeuCau
ADD CONSTRAINT FK_NhanVien_Duyet FOREIGN KEY (MaNV_Duyet) REFERENCES NhanVien(MaNV);


