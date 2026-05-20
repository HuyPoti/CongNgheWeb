namespace backend.Models;

public enum UserRole
{
    admin = 1,
    warehouse = 2, // Nhân viên kho: QC, đóng gói, in phiếu
    customer = 3,
    staff = 4      // Nhân viên thường: CSKH, xác nhận/hủy đơn, tra cứu
}
