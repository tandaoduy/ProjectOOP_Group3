using System.Collections.Generic;
using ProjectOOP_Group3.Modal;

namespace ProjectOOP_Group3.Areas.Admin.Models
{
    public class ThongKeViewModel
    {
        public int TongSo { get; set; }
        public int ChoDuyet { get; set; }
        public int DaDuyet { get; set; }
        public int TuChoi { get; set; }
        public List<PhieuYeuCau> PhieuYeuCaus { get; set; }
    }
}

