import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';
import { AlertifyService } from '../../shared/alertify.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private fb = inject(FormBuilder);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  constructor(
    private authService: AuthService,
    private alertify: AlertifyService,
    private router: Router
  ) {}

  submit() {
    if (this.loginForm.invalid) {
      return;
    }

    this.authService.login(this.loginForm.value as any).subscribe({
      next: () => {
        this.alertify.success('Giriş başarılı');
        this.router.navigate(['/events']);
      },
      error: (err) => {
        this.alertify.error(err.error?.detail ?? 'Giriş başarısız');
      }
    });
  }
}
