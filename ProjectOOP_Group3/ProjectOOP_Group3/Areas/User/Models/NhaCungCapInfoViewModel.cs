using System.Collections.Generic;
using ProjectOOP_Group3.Modal;

namespace ProjectOOP_Group3.Areas.User.Models
{
    public class NhaCungCapInfoViewModel
    {
        public string MaNCC { get; set; }
        public string TenNCC { get; set; }
        public List<ChiTietPYC> ChiTietPYCs { get; set; }
        public int SoLuongMatHang { get; set; }
        public decimal TongTien { get; set; }
    }
}

