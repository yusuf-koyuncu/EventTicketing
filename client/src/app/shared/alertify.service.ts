import { Injectable } from '@angular/core';

declare const alertify: any;

@Injectable({ providedIn: 'root' })
export class AlertifyService {
  success(message: string) {
    alertify.success(message);
  }

  error(message: string) {
    alertify.error(message);
  }

  message(message: string) {
    alertify.message(message);
  }

  confirm(message: string, onOk: () => void) {
    alertify.confirm(message, onOk);
  }
}
