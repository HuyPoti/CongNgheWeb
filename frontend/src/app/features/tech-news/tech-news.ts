import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

@Component({
  selector: 'app-tech-news',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './tech-news.html',
  styleUrl: './tech-news.css',
})
export class TechNews {
  featuredNews = {
    id: 1,
    title: 'The Future of Neural Rendering: RTX 50 Series Leaks',
    excerpt: 'Deep dive into next-gen Blackwell architecture and what it means for path-traced gaming.',
    category: 'Hardware',
    date: 'March 20, 2026',
    author: 'Admin Alpha',
    image: 'https://lh3.googleusercontent.com/aida-public/AB6AXuBPzThu9uEonp_i40D5SA5pcegCRyyySSmkTkcYjnhnbx-xSuglve6bU6zFH_opPtpzzBmLxJ6vhfnDXpA21sw3Cse0f3p-KyxTH66Y3xKZd2O_vaiN35OjISTDOgcop-okiQIAT8-wfI_0OmBYx2ZOGFBtHb0WZzjGxhf0kN-_NmZ5d2c5-AZ9uREbBwy5a5GiTcHT_0-cZuIhuW_rAOGxWGjeBIE5ayveBnscDdeXORGf0cZbJOYySRkfe10rONj0RpiP9b0q365d',
    tag: 'Trending'
  };

  newsList = [
    {
      id: 2,
      title: 'DDR6 Memory Standards Finalized',
      excerpt: 'JEDEC releases specifications for next-generation system memory with speeds up to 12800 MT/s.',
      category: 'Memory',
      date: 'March 18, 2026',
      image: 'https://lh3.googleusercontent.com/aida-public/AB6AXuBp-tO3xWBeb3zfbzG_DfHvADBqcwjQ2o-OFfFfnJ-ssgsrqN2uHExcRLoFQ-JTGJw6xSERTF7WGTcMIcYs71CXF3tbfgBrXkCs3WaZQc5EnNmmUPXFBU4_PG1jTqPN1f6Dbqze3BAR6gaynB7Iotj5SmDLcpJc5IVHHafqgVio5l4JtI89IMBI9o4RBh9ojmPhZTM2bD6KPsUrdkQ8kY3Z1IsoZbaKJrW1262HRoxLYGfRQ5Q0799Gg3SFy54cVZm-p1N-NwKxX1FI'
    },
    {
      id: 3,
      title: 'Linux Gaming Market Share Hits 5%',
      excerpt: 'Proton and Steam Deck continue to push alternative operating systems into the mainstream gaming space.',
      category: 'Software',
      date: 'March 15, 2026',
      image: 'https://lh3.googleusercontent.com/aida-public/AB6AXuCCn4gtyeJ2CaW7u2H189BkhzWyKr1mEWWeZrhgk-HzJFGA5bQugLrIlObsC-tcIfiF10HYYP4tsMhFZkPPCJoQmJhj0KS-a9b59ElgLyEuv7FtTwmnrIDvvWO6kmxbBv9qAqZve82LbSCSAicD9EQ6ND0w5mD-rrBWTAd-xywajRgVv3_GtYhrow0dDLfm8gy2uO13_1nKn-T5-tWf7HjRQqW4N3spxbK9Bns9ByXcyKgop0Z9VYxpLikYQJ0jfWEZwfkwp6yp8jZ2'
    },
    {
        id: 4,
        title: 'PCIe 7.0 Deployment in Data Centers',
        excerpt: 'Enterprise storage solutions begin adopting the ultra-bandwidth interface for next-gen AI training.',
        category: 'Interface',
        date: 'March 12, 2026',
        image: 'https://lh3.googleusercontent.com/aida-public/AB6AXuAgWijPBQPecdF0fcD6xuyJulf79DsZ4Rhx0zl_3gAeOwJW4zg3ePmaQ7__BaG7oEr6pkVQ9IvfMPs7QJtf3S0CLRWiqlzH-5ds8T7tiVE5NmUMee-vk4imksDZop3g6A07Q0fDjP8e6S_DUQr7DKZT3t0KQfSfweZguk5dcejfe7VbfC6_G3c6dI4a_PsesVw0HmEuTWtAaQvZEW9oygXJzdIZBgJClqP1zXUvcLXb35I8ZjFsVGvjN5rO9Sn_yVYlQBe2CA_JDkXQ'
    }
  ];

  categories = ['All', 'Hardware', 'Software', 'Gaming', 'AI', 'Deals'];
}
