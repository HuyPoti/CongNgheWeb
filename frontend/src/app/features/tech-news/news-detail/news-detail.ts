import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-news-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './news-detail.html',
  styleUrl: './news-detail.css'
})
export class NewsDetail {
  private route = inject(ActivatedRoute);
  
  // Mock news detail data
  article = signal({
    id: 1,
    title: 'The Rise of Quantum Neural Architecture',
    category: 'Hardware',
    date: 'March 20, 2026',
    author: 'Dr. Orion Vance',
    image: 'https://lh3.googleusercontent.com/aida-public/AB6AXuDFc2hHAI3Z1DDElMeRzO5-kZcQtq_csVK_VBN4KnJFo7xvm53XqayBerCkVmNZCUyw9KKYm1s4PWiIjGMknpx8cBLR7FnGByo6jtJ-BrGWO0aJ-QG003GhSge_JaNBrYoeYoeeWiKv8nny4z5a5Tec235ulUIHl26SB0slSQY1KlbRYlX3eg1kXVm9AphqKwCAqMOdxl4A3YK0PFpnIFG3qtqw8lSDEFgffgdRL_0xmIXMebJHZaQb3eBj2Op_0SJb6RzpIHhqHos7',
    content: [
      { type: 'p', text: 'In the rapidly evolving landscape of high-performance computing, the emergence of Quantum Neural Architecture (QNA) marks a pivotal shift in how we perceive processing power. Traditional silicon-based structures are reaching their thermal and physical limits, pushing researchers to explore the boundaries of sub-atomic logic gates.' },
      { type: 'h2', text: 'Breaking the 6nm Barrier' },
      { type: 'p', text: 'The latest ZX protocol chips utilize a hybrid approach, combining traditional logic with neural-entanglement modules. This allows for real-time data processing speeds previously limited to laboratory environments. By leveraging these new architectures, the upcoming RTX 50-series (codenamed "Caelum") promises a 400% increase in tensor core efficiency.' },
      { type: 'image', url: 'https://lh3.googleusercontent.com/aida-public/AB6AXuBWPHC83VT3lsEt_nQFA4ARGE1bzPdWb_IxWlepWAgbmv3kLkpN-VdVl72XADm3_KBQ0_PZv7IZfJvxSMnjlxzWFc3grSn9Qz34TSmvTVF9WVLZbQ7neAiQkumpq4ldvK4R9kHIkC07UTy0vPR3mnQwdObHCUsgVmICmdubrME-4E1-EA1QSUWXu8b1ir86Plcd6wmrf7JiIXhgMgvmNhEmumSRnmsMxlvZsHXjTsv02LPRSLZ5frTwVA6RV4fBK73KKzG9uAnx7fwZ', caption: 'Scanning electron micrograph of the new QNA-100 logic gate cluster.' },
      { type: 'p', text: 'Critics argue that the power requirements for such hardware will necessitate a complete overhaul of consumer-grade PSUs. "We aren\'t just looking at more watts," says Orion Vance, "we are looking at the need for stable, noise-free current that can sustain the high-frequency oscillation of these units."' },
      { type: 'h2', text: 'Impact on Development' },
      { type: 'p', text: 'For developers, this means the software-hardware bottleneck is effectively dissolved. Game engines can now handle trillion-polygon environments with path-tracing enabled at a native layer, removing the reliance on upscaling technologies like DLSS or FSR.' }
    ],
    tags: ['Quantum', 'Hardware', 'ZX-2026', 'AI Scaling']
  });

  constructor() {
    // In a real app, we would fetch the article by id
    const id = this.route.snapshot.paramMap.get('id');
    console.log('Fetching news ID:', id);
  }
}
