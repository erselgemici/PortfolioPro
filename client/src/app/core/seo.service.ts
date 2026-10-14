import { Injectable, inject } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';

// Her sayfada tarayıcı sekmesindeki başlığı ve meta description'ı güncelliyor.
// LinkedIn'de link paylaşınca çıkan önizleme de og: etiketlerinden geliyor.
@Injectable({ providedIn: 'root' })
export class SeoService {
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);

  set(pageTitle: string, description?: string): void {
    this.title.setTitle(pageTitle);
    this.meta.updateTag({ property: 'og:title', content: pageTitle });

    if (description) {
      const short = description.length > 160 ? description.slice(0, 157) + '...' : description;
      this.meta.updateTag({ name: 'description', content: short });
      this.meta.updateTag({ property: 'og:description', content: short });
    }
  }
}
