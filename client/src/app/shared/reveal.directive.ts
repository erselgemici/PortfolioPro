import { Directive, ElementRef, OnDestroy, OnInit, inject } from '@angular/core';

// Eleman ekrana girince yumuşak bir şekilde belirmesini sağlıyor.
// NOT: scroll event'ini dinlemek yerine IntersectionObserver kullandım. Scroll event'i saniyede
// onlarca kez tetiklenir, IntersectionObserver ise sadece eleman görünür olduğunda haber veriyor.
@Directive({ selector: '[appReveal]' })
export class RevealDirective implements OnInit, OnDestroy {
  private readonly el = inject<ElementRef<HTMLElement>>(ElementRef);
  private observer?: IntersectionObserver;

  ngOnInit(): void {
    const node = this.el.nativeElement;
    node.classList.add('reveal');

    if (!('IntersectionObserver' in window)) {
      node.classList.add('is-visible');
      return;
    }

    this.observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            node.classList.add('is-visible');
            this.observer?.disconnect(); // bir kez göründü, artık izlemeye gerek yok
          }
        }
      },
      { threshold: 0.12, rootMargin: '0px 0px -40px 0px' }
    );
    this.observer.observe(node);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }
}
