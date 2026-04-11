import { Review } from '../models/product.model';

// Mock products đã được xoá – dữ liệu lấy từ API thực
export const MOCK_PRODUCTS: any[] = [];

// Reviews giữ static vì backend chưa có API reviews
export const MOCK_REVIEWS: Review[] = [
  {
    id: '1',
    userName: 'Nguyễn Văn An',
    userInitial: 'N',
    rating: 5,
    date: 'March 2026',
    title: 'Sản phẩm chất lượng cao, giao hàng nhanh',
    body: 'Hiệu năng vượt trội so với mức giá. Đóng gói cẩn thận, giao hàng đúng hẹn. Rất hài lòng với trải nghiệm mua sắm.',
    product: 'Product',
    verified: true,
    helpful: 14,
    images: [],
  },
  {
    id: '2',
    userName: 'Trần Minh Khoa',
    userInitial: 'T',
    rating: 4,
    date: 'February 2026',
    title: 'Đáng mua, chất lượng tốt',
    body: 'Sản phẩm chắc chắn, hoạt động ổn định. Giá hơi cao nhưng xứng đáng với chất lượng. Sẽ mua lại lần sau.',
    product: 'Product',
    verified: true,
    helpful: 9,
    images: [],
  },
  {
    id: '3',
    userName: 'Lê Thị Hương',
    userInitial: 'L',
    rating: 5,
    date: 'January 2026',
    title: 'Tuyệt vời! Vượt ngoài mong đợi',
    body: 'Mình đã dùng nhiều sản phẩm cùng loại nhưng cái này thực sự nổi bật. Hiệu suất ổn định, không có vấn đề gì sau 2 tháng sử dụng.',
    product: 'Product',
    verified: false,
    helpful: 6,
    images: [],
  },
];
