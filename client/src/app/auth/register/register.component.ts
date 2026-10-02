import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';
import { AlertifyService } from '../../shared/alertify.service';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  private fb = inject(FormBuilder);

  registerForm = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    phoneNumber: ['']
  });

  constructor(
    private authService: AuthService,
    private alertify: AlertifyService,
    private router: Router
  ) {}

  submit() {
    if (this.registerForm.invalid) {
      return;
    }

    const { fullName, email, password, phoneNumber } = this.registerForm.value;

    this.authService
      .register({
        fullName: fullName!,
        email: email!,
        password: password!,
        phoneNumber: phoneNumber ? phoneNumber : undefined
      })
      .subscribe({
      next: () => {
        this.alertify.success('Kayıt başarılı');
        this.router.navigate(['/events']);
      },
      error: (err) => {
        this.alertify.error(err.error?.detail ?? 'Kayıt başarısız');
      }
    });
  }
}
