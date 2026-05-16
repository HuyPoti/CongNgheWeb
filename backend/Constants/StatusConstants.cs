namespace backend.Constants;

public static class OrderStatus
{
    public const int Pending = 1;
    public const int Confirmed = 2;
    public const int Processing = 3;
    public const int Shipping = 4;
    public const int Delivered = 5;
    public const int Cancelled = 6;
}

public static class PaymentStatus
{
    public const int Pending = 1;
    public const int Completed = 2;
    public const int Failed = 3;
    public const int Refunded = 4;
}

public static class ProductStatus
{
    public const int Draft = 1;
    public const int Published = 2;
    public const int Deleted = 3;
}

public static class UserRoleConstants
{
    public const int Admin = 1;
    public const int Staff = 2;
    public const int Customer = 3;
}

public static class BannerPositionConstants
{
    public const int HomepageSlider = 0;
    public const int HomepageMid = 1;
    public const int CategoryTop = 2;
    public const int NewsTop = 3;
}

public static class ReturnRequestStatus
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Completed = "completed";
}

public static class ShipmentStatus
{
    public const string Pending = "pending";
    public const string Packing = "packing";
    public const string Packed = "packed";
    public const string QcPassed = "qc_passed";
    public const string Shipping = "shipping";
    public const string Delivered = "delivered";
}

public static class CouponDiscountType
{
    public const string Percentage = "percentage";
    public const string Fixed = "fixed";
}

public static class ReviewStatus
{
    public const int Published = 1;
    public const int Hidden = 2;
}

public static class InventoryReceiptStatus
{
    public const int Draft = 1;
    public const int Completed = 2;
    public const int Cancelled = 3;
}

public static class InventoryTransactionType
{
    public const int Import = 1;     // Nhập kho
    public const int Export = 2;     // Xuất bán
    public const int Return = 3;    // Hoàn hàng
    public const int Dispose = 4;   // Xuất hủy
}
