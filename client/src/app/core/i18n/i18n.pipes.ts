import { Pipe, PipeTransform, inject } from '@angular/core';
import { I18nService } from './i18n.service';
import { TranslationKey } from './translations';
import { Localized } from '../models';

// {{ 'nav.about' | t }}
// NOT: pure: false yaptım. Pipe'a giden değer ('nav.about') hiç değişmiyor ama dil değişince sonucun değişmesi lazım.
// pure olsaydı Angular "değer aynı" deyip pipe'ı tekrar çalıştırmazdı ve ekrandaki dil değişmezdi.
@Pipe({ name: 't', pure: false })
export class TranslatePipe implements PipeTransform {
  private readonly i18n = inject(I18nService);

  transform(key: TranslationKey): string {
    return this.i18n.t(key);
  }
}

// {{ project.title | loc }}  ->  seçili dile göre tr ya da en
@Pipe({ name: 'loc', pure: false })
export class LocalizePipe implements PipeTransform {
  private readonly i18n = inject(I18nService);

  transform(value: Localized | null | undefined): string {
    return this.i18n.pick(value);
  }
}
