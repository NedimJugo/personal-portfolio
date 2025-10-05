import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'phoneLink', standalone: true })
export class PhoneLinkPipe implements PipeTransform {
  transform(value: string): string {
    return value ? 'tel:' + value.replace(/[^0-9+]/g, '') : '';
  }
}
