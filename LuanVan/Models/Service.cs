using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using X.PagedList;
using LuanVan.Data;
using Microsoft.AspNetCore.Identity;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.InkML;

namespace LuanVan.Models
{
    public class Service
    {
        private ApplicationDbContext _context = new ApplicationDbContext();
        public (SanPham[] sanphams, int pages, int page) Paging(int page)
        {
            int size = 2;
            int pages = (int)Math.Ceiling((double)_context.SanPhams.Count() / size);
            var sanPhams = _context.SanPhams.Skip((page - 1) * size).Take(size).ToArray();
            return (sanPhams, pages, page);
        }
        public DateTime DateTimeVN()
        {
            DateTime utcTime = DateTime.UtcNow; // Lấy thời gian hiện tại theo giờ UTC
            TimeZoneInfo vietnamZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Lấy thông tin về múi giờ của Việt Nam
            DateTime vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, vietnamZone); // Chuyển đổi giá trị DateTime từ múi giờ UTC sang múi giờ của Việt Nam

            return vietnamTime;
        }
        public IQueryable<LoaiSanPham> danhSachLoaiSP()
        {
            return _context.LoaiSanPhams.Where(x => x.MaLoaiSp != null);
        }
        public async Task<string> getTenLoaiSP(string maLoai)
        {
            return await _context.LoaiSanPhams.Where(x => x.MaLoaiSp == maLoai).Select(x => x.TenLoaiSp).FirstOrDefaultAsync();
        }
        public IQueryable<GioHang> danhSachGioHang(int tt = 2, string? maKhachHang = null)
        {
            var rs = _context.GioHangs.Where(x => x.KhachHangId == maKhachHang && x.SanPham.TrangThai != -1 && x.SanPham.TrangThai != 0 && x.SanPham.SoLuongTon > 0);
            if (tt == 2)
            {
                return rs.Where(x => x.MaGioHang != null).Include(x => x.SanPham);
            }

            return rs.Where(x => x.TrangThai == tt).Include(x => x.SanPham);
        }
        public async Task<GioHang> getGioHang(string maGioHang)
        {
            if (!string.IsNullOrEmpty(maGioHang))
            {
                return await _context.GioHangs.Where(x => x.MaGioHang == maGioHang).Include(x => x.SanPham).FirstOrDefaultAsync();
            }
            return null;
        }
        public async Task<string> themGioHang(string maSP, string? maKH = null)
        {
            string maGH = "GH" + DateTimeVN().ToString("ddMMyyyyHHmmss") + 3;
            var rs = danhSachGioHang(0, maKH).Where(x => x.MaSanPham == maSP);
            if (rs.Any())
            {
                var model = rs.FirstOrDefault();
                _context.Update(model);
                model.SoLuongDat = model.SoLuongDat + 1;
                await _context.SaveChangesAsync();
                return model.MaGioHang;
            }

            GioHang gh = new GioHang();
            gh.MaGioHang = maGH;
            gh.KhachHangId = maKH;
            if (maKH == null)
            {
                gh.TrangThai = 2;
            }
            else
            {
                gh.TrangThai = 0;
            }
            gh.SoLuongDat = 1;
            gh.MaSanPham = maSP;
            _context.GioHangs.Add(gh);
            await _context.SaveChangesAsync();
            return gh.MaGioHang;
        }
        public async Task xoaGioHang(string maGH)
        {
            if (!string.IsNullOrEmpty(maGH))
            {
                var model = _context.GioHangs.Find(maGH);
                _context.Update(model);
                if (model.TrangThai == 0 || model.TrangThai == 2)
                {
                    model.TrangThai = -1;
                }
                await _context.SaveChangesAsync();
            }
        }
        public IQueryable<SanPham> danhSachSanPham()
        {
            return (from a in _context.SanPhams
                    where (a.TrangThai == 1 || a.TrangThai == 2) && a.SoLuongTon > 0
                    orderby a.TenSanPham ascending
                    select a);

        }
        public IQueryable<SanPham> danhSachSanPhamHot()
        {
            return _context.SanPhams.Where(x => x.MaSanPham != null).Where(x => x.TrangThai == 2).Where(x => x.SoLuongTon > 0);
        }
        public IQueryable<SanPham> danhSachSanPhamKhuyenMai()
        {
            return _context.SanPhams.Where(x => x.MaSanPham != null).Where(x => x.TrangThai == 1).Where(x => x.SoLuongTon > 0);
        }
        public IQueryable<SanPham> danhSachSanPhamBanChay()
        {
            var result = (_context.SanPhams
                            .Where(x => x.SoLuongDaBan > 0 && x.TrangThai != -1)
                            .Where(x => x.SoLuongTon > 0)
                            .OrderByDescending(sp => sp.SoLuongDaBan));
            return result;
        }
        public IQueryable<SanPham> danhSachSanPham(string maLoai)
        {
            return _context.SanPhams.Where(x => (x.TrangThai == 1 && x.MaLoaiSp == maLoai) ||
                                                 (x.TrangThai == 2 && x.MaLoaiSp == maLoai));
        }
        public async Task<SanPham?> getSanPham(string maSanPham)
        {
            return await danhSachSanPham().Where(x => x.MaSanPham == maSanPham).FirstOrDefaultAsync();
        }
        public async Task<SanPham?> GetSanPham(string maSanPham)
        {
            return await _context.SanPhams.FindAsync(maSanPham);
        }
        public IQueryable<SanPham> timKiem(string key)
        {
            return danhSachSanPham().Where(x => x.TenSanPham.Contains(key));
        }
        public async Task<KhachHang> getKH(string maKH)
        {
            return await _context.KhachHangs.FindAsync(maKH);
        }
        public async Task<string> getMaPTTT(string maHD)
        {
            var result = _context.HoaDons
                .Where(hd => hd.MaHoaDon == maHD)
                .Select(hd => hd.MaPttt)
                .FirstOrDefaultAsync();
            return await result;
        }
        public async Task suaTrangThaiThanhToan(string maHD, int trangThai)
        {
            var hoaDon = await getHoaDon(maHD);
            _context.Update(hoaDon);
            hoaDon.TrangThaiThanhToan = trangThai;
            await _context.SaveChangesAsync();
        }

        public async Task huyDonHang(string maHD)
        {
            var hoaDon = await getHoaDon(maHD);
            _context.Update(hoaDon);
            hoaDon.TrangThaiDonHang = -1;
            await _context.SaveChangesAsync();
        }

        public async Task suaTrangThaiDonHang(string maHD, int trangThai)
        {
            var hoaDon = await getHoaDon(maHD);
            _context.Update(hoaDon);
            hoaDon.TrangThaiDonHang = trangThai;
            await _context.SaveChangesAsync();
        }
        public IQueryable<HoaDon> danhSachHoaDon(string maKH = null)
        {
            if (maKH == null)
            {
                return _context.HoaDons.Where(x => x.MaHoaDon != null).Include(x => x.ChiTietHds);
            }
            return _context.HoaDons.Where(x => x.KhachHangId == maKH).Include(x => x.ChiTietHds);
        }
        public async Task<HoaDon> getHoaDon(string maHD)
        {
            return await danhSachHoaDon().Where(x => x.MaHoaDon == maHD).FirstOrDefaultAsync();
        }
        public async Task<string> GetMaKM(string mahd)
        {
            var maKM = await _context.HoaDons
                .Where(a => a.MaHoaDon == mahd)
                .Select(a => a.MaKm)
                .FirstOrDefaultAsync();

            return maKM;
        }
        public async Task<KhuyenMai> GetKhuyenMai(string makm)
        {
            var query = await _context.KhuyenMais
                .Where(a => a.MaKm == makm)
                .FirstOrDefaultAsync();
            return query;
        }
        public async Task TangHangTon(string orderId)
        {
            var chiTietHoaDons = await _context.ChiTietHds.Where(x => x.MaHoaDon == orderId).ToListAsync();

            foreach (var chiTietHoaDon in chiTietHoaDons)
            {
                var gioHang = await _context.GioHangs.FirstOrDefaultAsync(x => x.MaGioHang == chiTietHoaDon.MaGioHang);
                var sanPham = await _context.SanPhams.FirstOrDefaultAsync(x => x.MaSanPham == gioHang.MaSanPham);

                if (sanPham != null && gioHang != null)
                {
                    sanPham.SoLuongTon += gioHang.SoLuongDat;
                    _context.Update(sanPham);
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task GiamHangTon(string orderId)
        {
            var chiTietHoaDons = await _context.ChiTietHds.Where(x => x.MaHoaDon == orderId).ToListAsync();

            foreach (var chiTietHoaDon in chiTietHoaDons)
            {
                var gioHang = await _context.GioHangs.FirstOrDefaultAsync(x => x.MaGioHang == chiTietHoaDon.MaGioHang);
                var sanPham = await _context.SanPhams.FirstOrDefaultAsync(x => x.MaSanPham == gioHang.MaSanPham);

                if (sanPham != null && gioHang != null)
                {
                    sanPham.SoLuongTon -= gioHang.SoLuongDat;
                    _context.Update(sanPham);
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<string> themChiTietHD(string maHD, string maGH)
        {
            ChiTietHd chiTietHD = new ChiTietHd();
            chiTietHD.MaChiTietHd = "" + DateTimeVN().Ticks.ToString() + 3;
            chiTietHD.MaGioHang = maGH;
            var gioHang = await getGioHang(maGH);
            gioHang.TrangThai = 1;
            chiTietHD.MaHoaDon = maHD;
            _context.ChiTietHds.Add(chiTietHD);
            await _context.SaveChangesAsync();
            return chiTietHD.MaChiTietHd;
        }
        public async Task increase(string maGH, int soLuong)
        {
            var gioHang = await getGioHang(maGH);
            _context.Update(gioHang);
            gioHang.SoLuongDat = soLuong;

            await _context.SaveChangesAsync();
        }
        public async Task<string?> getNhaSXBySanPham(string maSP)
        {
            var result = (from a in _context.SanPhams
                          join b in _context.NhaSanXuats on a.MaNsx equals b.MaNsx
                          where a.MaSanPham == maSP
                          select b.TenNsx).FirstOrDefaultAsync();
            return await result;

        }
        public async Task<string?> getMauSacBySanPham(string maSP)
        {
            var result = (from a in _context.SanPhams
                          join b in _context.MauSacs on a.MaMau equals b.MaMau
                          where a.MaSanPham == maSP
                          select b.TenMau).FirstOrDefaultAsync();
            return await result;

        }
        public async Task<string?> getMotaBySanPham(string maSP)
        {
            var result = (from a in _context.SanPhams
                          where a.MaSanPham == maSP
                          select a.MoTa).FirstOrDefaultAsync();
            return await result;
        }
        public async Task<IPagedList<SanPham>> PagingSortProductByName(string input, bool isAscending, int page, int size)
        {
            var result = (from a in _context.SanPhams
                          where a.MaLoaiSp == input
                          orderby a.TenSanPham ascending
                          select a).ToPagedListAsync(page, size);
            if (!isAscending) // Nếu isAscending là false thì sắp xếp giảm dần
            {
                result = (from a in _context.SanPhams
                          where a.MaLoaiSp == input
                          orderby a.TenSanPham descending
                          select a).ToPagedListAsync(page, size);
            }
            return await result;
        }
        public async Task<IPagedList<SanPham>> PagingProductByLoaiSP(string maloai, int page, int size)
        {
            Task<IPagedList<SanPham>> result;
            Console.WriteLine("MALOAI:"+ maloai);
            if (maloai.Equals(""))
            {
                result = danhSachSanPham().ToPagedListAsync(page, size);
            }
            else
            {
                result = danhSachSanPham(maloai).ToPagedListAsync(page, size);

            }

            return await result;
        }
        public async Task<IPagedList<SanPham>> PagingSortProductByPrice(string input, bool isAscending, int page, int size)
        {

            var result = (from a in _context.SanPhams
                          where a.MaLoaiSp == input
                          orderby a.GiaBan ascending
                          select a).ToPagedListAsync(page, size);
            if (!isAscending) // Nếu isAscending là false thì sắp xếp giảm dần
            {
                result = (from a in _context.SanPhams
                          where a.MaLoaiSp == input
                          orderby a.GiaBan descending
                          select a).ToPagedListAsync(page, size);
            }
            return await result;
        }
        public async Task<IPagedList<SanPham>> PagingSanPhams(int page, int size)
        {
            return await danhSachSanPham().ToPagedListAsync(page, size);
        }

        public async Task<IPagedList<SanPham>> PagingSanPhamsByLoaiSP(string loaisp, int page, int size)
        {
            return await danhSachSanPham(loaisp).ToPagedListAsync(page, size);
        }

        public async Task<IPagedList<SanPham>> PagingSanPhamsByKey(string key, int page, int size)
        {
            return await timKiem(key).ToPagedListAsync(page, size);
        }
    }
}