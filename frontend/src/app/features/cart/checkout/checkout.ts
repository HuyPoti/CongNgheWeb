import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { ToastService } from '../../../core/services/toast.service';

interface Province {
  code: string;
  name: string;
}

interface District {
  code: string;
  name: string;
}

interface Ward {
  code: string;
  name: string;
}

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css',
})
export class Checkout {
  private router = inject(Router);
  private toastService = inject(ToastService);
  cartService = inject(CartService);

  firstName = '';
  lastName = '';
  phone = '';
  addressLine = '';

  provinces: Province[] = [];
  districts: District[] = [];
  wards: Ward[] = [];

  selectedProvince = '';
  selectedDistrict = '';
  selectedWard = '';

  constructor() {
    this.loadProvinces();
  }

  loadProvinces() {
    this.provinces = [
      { code: '01', name: 'Thành phố Hà Nội' },
      { code: '02', name: 'Thành phố Hồ Chí Minh' },
      { code: '03', name: 'Thành phố Đà Nẵng' },
      { code: '04', name: 'Thành phố Hải Phòng' },
      { code: '05', name: 'Thành phố Cần Thơ' },
      { code: '06', name: 'Tỉnh Hà Giang' },
      { code: '07', name: 'Tỉnh Cao Bằng' },
      { code: '08', name: 'Tỉnh Bắc Kạn' },
      { code: '09', name: 'Tỉnh Tuyên Quang' },
      { code: '10', name: 'Tỉnh Lào Cai' },
      { code: '11', name: 'Tỉnh Điện Biên' },
      { code: '12', name: 'Tỉnh Lai Châu' },
      { code: '13', name: 'Tỉnh Sơn La' },
      { code: '14', name: 'Tỉnh Yên Bái' },
      { code: '15', name: 'Tỉnh Thái Nguyên' },
      { code: '16', name: 'Tỉnh Lạng Sơn' },
      { code: '17', name: 'Tỉnh Quảng Ninh' },
      { code: '18', name: 'Tỉnh Bắc Giang' },
      { code: '19', name: 'Tỉnh Phú Thọ' },
      { code: '20', name: 'Tỉnh Vĩnh Phúc' },
      { code: '21', name: 'Tỉnh Bắc Ninh' },
      { code: '22', name: 'Tỉnh Hải Dương' },
      { code: '23', name: 'Thành phố Hải Phòng' },
      { code: '24', name: 'Tỉnh Hưng Yên' },
      { code: '25', name: 'Tỉnh Thái Bình' },
      { code: '26', name: 'Tỉnh Hà Nam' },
      { code: '27', name: 'Tỉnh Nam Định' },
      { code: '28', name: 'Tỉnh Ninh Bình' },
      { code: '29', name: 'Tỉnh Thanh Hóa' },
      { code: '30', name: 'Tỉnh Nghệ An' },
      { code: '31', name: 'Tỉnh Hà Tĩnh' },
      { code: '32', name: 'Tỉnh Quảng Bình' },
      { code: '33', name: 'Tỉnh Quảng Trị' },
      { code: '34', name: 'Tỉnh Thừa Thiên Huế' },
      { code: '35', name: 'Thành phố Đà Nẵng' },
      { code: '36', name: 'Tỉnh Quảng Nam' },
      { code: '37', name: 'Tỉnh Quảng Ngãi' },
      { code: '38', name: 'Tỉnh Bình Định' },
      { code: '39', name: 'Tỉnh Phú Yên' },
      { code: '40', name: 'Tỉnh Khánh Hòa' },
      { code: '41', name: 'Tỉnh Ninh Thuận' },
      { code: '42', name: 'Tỉnh Bình Thuận' },
      { code: '43', name: 'Tỉnh Kon Tum' },
      { code: '44', name: 'Tỉnh Gia Lai' },
      { code: '45', name: 'Tỉnh Đắk Lắk' },
      { code: '46', name: 'Tỉnh Đắk Nông' },
      { code: '47', name: 'Tỉnh Lâm Đồng' },
      { code: '48', name: 'Tỉnh Bình Phước' },
      { code: '49', name: 'Tỉnh Tây Ninh' },
      { code: '50', name: 'Tỉnh Bình Dương' },
      { code: '51', name: 'Tỉnh Đồng Nai' },
      { code: '52', name: 'Tỉnh Bà Rịa - Vũng Tàu' },
      { code: '53', name: 'Thành phố Hồ Chí Minh' },
      { code: '54', name: 'Tỉnh Long An' },
      { code: '55', name: 'Tỉnh Tiền Giang' },
      { code: '56', name: 'Tỉnh Bến Tre' },
      { code: '57', name: 'Tỉnh Trà Vinh' },
      { code: '58', name: 'Tỉnh Vĩnh Long' },
      { code: '59', name: 'Tỉnh Đồng Tháp' },
      { code: '60', name: 'Tỉnh An Giang' },
      { code: '61', name: 'Tỉnh Kiên Giang' },
      { code: '62', name: 'Thành phố Cần Thơ' },
      { code: '63', name: 'Tỉnh Hậu Giang' },
      { code: '64', name: 'Tỉnh Sóc Trăng' },
      { code: '65', name: 'Tỉnh Bạc Liêu' },
      { code: '66', name: 'Tỉnh Cà Mau' },
    ];
  }

  onProvinceChange() {
    this.districts = this.getDistrictsByProvince(this.selectedProvince);
    this.wards = [];
    this.selectedDistrict = '';
    this.selectedWard = '';
  }

  onDistrictChange() {
    this.wards = this.getWardsByDistrict(this.selectedDistrict);
    this.selectedWard = '';
  }

  getDistrictsByProvince(provinceCode: string): District[] {
    const districtsMap: Record<string, District[]> = {
      '01': [
        { code: '001', name: 'Quận Ba Đình' },
        { code: '002', name: 'Quận Hoàn Kiếm' },
        { code: '003', name: 'Quận Tây Hồ' },
        { code: '004', name: 'Quận Long Biên' },
        { code: '005', name: 'Quận Cầu Giấy' },
        { code: '006', name: 'Quận Đống Đa' },
        { code: '007', name: 'Quận Hai Bà Trưng' },
        { code: '008', name: 'Quận Thanh Xuân' },
        { code: '009', name: 'Quận Hoàng Mai' },
        { code: '010', name: 'Quận Nam Từ Liêm' },
        { code: '011', name: 'Quận Bắc Từ Liêm' },
        { code: '012', name: 'Huyện Mỹ Đức' },
        { code: '013', name: 'Huyện Ứng Hòa' },
        { code: '014', name: 'Huyện Thường Tín' },
        { code: '015', name: 'Huyện Phú Xuyên' },
        { code: '016', name: 'Huyện Thanh Oai' },
        { code: '017', name: 'Huyện Chương Mỹ' },
        { code: '018', name: 'Huyện Đan Phượng' },
        { code: '019', name: 'Huyện Hoài Đức' },
        { code: '020', name: 'Huyện Quốc Oai' },
        { code: '021', name: 'Huyện Thạch Thất' },
        { code: '022', name: 'Huyện Phúc Thọ' },
        { code: '023', name: 'Huyện Sơn Tây' },
        { code: '024', name: 'Huyện Ba Vì' },
        { code: '025', name: 'Huyện Vĩnh Tường' },
        { code: '026', name: 'Huyện Yên Lãng' },
        { code: '027', name: 'Huyện Đông Anh' },
        { code: '028', name: 'Huyện Sóc Sơn' },
        { code: '029', name: 'Quận Hà Đông' },
        { code: '030', name: 'Thị xã Sơn Tây' },
      ],
      '02': [
        { code: '761', name: 'Quận 1' },
        { code: '762', name: 'Quận 12' },
        { code: '763', name: 'Quận Gò Vấp' },
        { code: '764', name: 'Quận Bình Thạnh' },
        { code: '765', name: 'Quận Tân Bình' },
        { code: '766', name: 'Quận Tân Phú' },
        { code: '767', name: 'Quận Phú Nhuận' },
        { code: '768', name: 'Quận Thủ Đức' },
        { code: '769', name: 'Quận 3' },
        { code: '770', name: 'Quận 10' },
        { code: '771', name: 'Quận 11' },
        { code: '772', name: 'Quận 4' },
        { code: '773', name: 'Quận 5' },
        { code: '774', name: 'Quận 6' },
        { code: '775', name: 'Quận 8' },
        { code: '776', name: 'Quận Bình Tân' },
        { code: '777', name: 'Quận 7' },
        { code: '778', name: 'Huyện Củ Chi' },
        { code: '779', name: 'Huyện Hóc Môn' },
        { code: '780', name: 'Huyện Bình Chánh' },
        { code: '781', name: 'Huyện Nhà Bè' },
        { code: '782', name: 'Huyện Cần Giờ' },
      ],
      '03': [
        { code: '501', name: 'Quận Hải Châu' },
        { code: '502', name: 'Quận Thanh Khê' },
        { code: '503', name: 'Quận Sơn Trà' },
        { code: '504', name: 'Quận Ngũ Hành Sơn' },
        { code: '505', name: 'Quận Liên Chiểu' },
        { code: '506', name: 'Huyện Hòa Vang' },
        { code: '507', name: 'Huyện Hoàng Sa' },
      ],
    };
    return districtsMap[provinceCode] || [];
  }

  getWardsByDistrict(districtCode: string): Ward[] {
    const wardsMap: Record<string, Ward[]> = {
      '001': [
        { code: '00001', name: 'Phường Phúc Xá' },
        { code: '00002', name: 'Phường Trúc Bạch' },
        { code: '00003', name: 'Phường Vĩnh Phúc' },
        { code: '00004', name: 'Phường Cống Vị' },
        { code: '00005', name: 'Phường Liễu Giai' },
        { code: '00006', name: 'Phường Ngọc Hà' },
        { code: '00007', name: 'Phường Ngọc Khánh' },
        { code: '00008', name: 'Phường Quán Thán' },
        { code: '00009', name: 'Phường Tứ Liên' },
        { code: '00010', name: 'Phường Thượng Thanh' },
      ],
      '761': [
        { code: '26701', name: 'Phường Tân Định' },
        { code: '26703', name: 'Phường Đa Kao' },
        { code: '26706', name: 'Phường Bến Nghé' },
        { code: '26709', name: 'Phường Bến Thành' },
        { code: '26712', name: 'Phường Nguyễn Thái Bình' },
        { code: '26715', name: 'Phường Phạm Ngũ Lão' },
        { code: '26718', name: 'Phường Cầu Ông Lãnh' },
        { code: '26721', name: 'Phường Cổ Giam' },
        { code: '26724', name: 'Phường Nguyễn Cư Trinh' },
        { code: '26727', name: 'Phường Lê Đình Chiêm' },
      ],
      '501': [
        { code: '20281', name: 'Phường Thanh Khê Tây' },
        { code: '20282', name: 'Phường Thanh Khê Đông' },
        { code: '20284', name: 'Phường Xuân Hà' },
        { code: '20285', name: 'Phường Hòa Khê' },
        { code: '20287', name: 'Phường Tam Thuận' },
        { code: '20288', name: 'Phường Thanh Khê' },
        { code: '20290', name: 'Phường An Khê' },
        { code: '20291', name: 'Phường Hòa An' },
        { code: '20293', name: 'Phường Hòa Phát' },
        { code: '20294', name: 'Phường Hòa Thọ Tây' },
      ],
    };
    return wardsMap[districtCode] || [];
  }

  get fullAddress(): string {
    const parts = [
      this.addressLine,
      this.selectedWard ? this.getWardName() : '',
      this.selectedDistrict ? this.getDistrictName() : '',
      this.selectedProvince ? this.getProvinceName() : '',
    ].filter(Boolean);
    return parts.join(', ');
  }

  getProvinceName(): string {
    const province = this.provinces.find(p => p.code === this.selectedProvince);
    return province?.name || '';
  }

  getDistrictName(): string {
    const district = this.districts.find(d => d.code === this.selectedDistrict);
    return district?.name || '';
  }

  getWardName(): string {
    const ward = this.wards.find(w => w.code === this.selectedWard);
    return ward?.name || '';
  }

  proceedToPayment() {
    if (!this.firstName || !this.lastName || !this.phone || !this.addressLine || !this.selectedProvince) {
      this.toastService.warning('Vui lòng điền đầy đủ thông tin giao hàng');
      return;
    }

    void this.router.navigate(['/cart/payment'], {
      state: {
        shippingAddress: {
          recipientName: `${this.firstName} ${this.lastName}`.trim(),
          phone: this.phone.trim(),
          addressLine: this.fullAddress,
          province: this.selectedProvince,
          district: this.selectedDistrict,
          ward: this.selectedWard,
        }
      }
    });
  }
}
