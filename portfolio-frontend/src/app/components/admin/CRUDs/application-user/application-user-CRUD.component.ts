import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil, finalize, debounceTime, skip } from 'rxjs';
import { ApplicationUserResponse } from '../../../../models/application-user/application-user-response.model';
import { ApplicationUserService } from '../../../../services/application-user.service';
import { ApplicationUserSearchObject } from '../../../../models/application-user/application-user-search.model';
import { ApplicationUserInsertRequest } from '../../../../models/application-user/application-user-insert-request.model';
import { ApplicationUserUpdateRequest } from '../../../../models/application-user/application-user-update-request.model';

function strongPasswordValidator(control: any) {
    const password = control.value;
    if (!password) return null;

    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumeric = /[0-9]/.test(password);
    const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(password);
    const isLengthValid = password.length >= 8;

    const passwordValid = hasUpperCase && hasLowerCase && hasNumeric && hasSpecialChar && isLengthValid;

    return !passwordValid ? { weakPassword: true } : null;
}

@Component({
    selector: 'app-application-user-crud',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './application-user-CRUD.component.html',
    styleUrls: ['./application-user-CRUD.component.scss']
})
export class ApplicationUserCrudComponent implements OnInit, OnDestroy {
    searchForm!: FormGroup;
    userForm!: FormGroup;

    private destroy$ = new Subject<void>();

    users: ApplicationUserResponse[] = [];
    totalCount: number | undefined;
    currentPage = 0;

    showPassword = false;
    showConfirmPassword = false;
    passwordStrength: 'weak' | 'medium' | 'strong' | null = null;

    searchParams: ApplicationUserSearchObject = {
        page: 0,
        pageSize: 20,
        sortBy: 'createdAt',
        desc: true,
        includeTotalCount: true,
        retrieveAll: false
    };

    isLoading = false;
    isSaving = false;
    isDeleting = false;
    showModal = false;
    showDeleteModal = false;
    isEditMode = false;
    formError: string | null = null;
    successMessage: string | null = null;

    currentUserId: number | null = null;
    userToDelete: ApplicationUserResponse | null = null;

    constructor(
        private userService: ApplicationUserService,
        private fb: FormBuilder
    ) { }

    ngOnInit(): void {
        this.initSearchForm();
        this.initUserForm();
        this.loadUsers();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private initSearchForm(): void {
        this.searchForm = this.fb.group({
            userName: [''],
            email: [''],
            isActive: [''],
            sortBy: ['createdAt'],
            desc: [true]
        });

        this.searchForm.valueChanges
            .pipe(
                takeUntil(this.destroy$),
                debounceTime(300),
                skip(1)
            )
            .subscribe(() => {
                this.onSearchChange();
            });
    }

    private initUserForm(): void {
        this.userForm = this.fb.group({
            userName: ['', [Validators.required, Validators.minLength(3)]],
            email: ['', [Validators.required, Validators.email]],
            fullName: ['', [Validators.required, Validators.minLength(2)]],
            isActive: [true],
            password: [''],
            confirmPassword: ['']
        }, {
            validators: this.passwordMatchValidator
        });

        this.userForm.get('password')?.valueChanges
            .pipe(takeUntil(this.destroy$))
            .subscribe(password => {
                this.calculatePasswordStrength(password);
            });
    }

    // Custom validator to check if passwords match
    private passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
        const password = group.get('password')?.value;
        const confirmPassword = group.get('confirmPassword')?.value;

        // Only validate if password field has a value
        if (password && password !== confirmPassword) {
            return { passwordMismatch: true };
        }
        return null;
    }

    loadUsers(): void {
        this.isLoading = true;

        this.userService.get(this.searchParams)
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isLoading = false)
            )
            .subscribe({
                next: (result) => {
                    this.users = result.items;
                    this.totalCount = result.totalCount;
                },
                error: (error) => {
                    console.error('Failed to load users:', error);
                    this.formError = 'Failed to load users. Please try again.';
                }
            });
    }

    private updateSearchParamsFromForm(): void {
        const formValue = this.searchForm.value;

        let isActiveValue: boolean | undefined;
        if (formValue.isActive === true || formValue.isActive === 'true') {
            isActiveValue = true;
        } else if (formValue.isActive === false || formValue.isActive === 'false') {
            isActiveValue = false;
        } else {
            isActiveValue = undefined;
        }

        this.searchParams = {
            ...this.searchParams,
            userName: formValue.userName?.trim() || undefined,
            email: formValue.email?.trim() || undefined,
            isActive: isActiveValue,
            sortBy: formValue.sortBy || 'createdAt',
            desc: formValue.desc || false,
            page: 0
        };
    }

    onSearchChange(): void {
        this.currentPage = 0;
        this.updateSearchParamsFromForm();
        this.loadUsers();
    }

    toggleSortOrder(): void {
        const currentDesc = this.searchForm.get('desc')?.value;
        this.searchForm.patchValue({ desc: !currentDesc });
    }

    hasActiveFilters(): boolean {
        const formValue = this.searchForm.value;
        return !!(
            formValue.userName ||
            formValue.email ||
            formValue.isActive !== ''
        );
    }

    clearFilters(): void {
        this.searchForm.patchValue({
            userName: '',
            email: '',
            isActive: '',
            sortBy: 'createdAt',
            desc: true
        });
    }

    goToPage(page: number): void {
        this.currentPage = page;
        this.searchParams.page = page;
        this.loadUsers();
    }

    getTotalPages(): number {
        if (!this.totalCount) return 1;
        return Math.ceil(this.totalCount / (this.searchParams.pageSize || 20));
    }

    getPageNumbers(): number[] {
        const totalPages = this.getTotalPages();
        const pages: number[] = [];
        const maxVisible = 5;

        let start = Math.max(0, this.currentPage - Math.floor(maxVisible / 2));
        let end = Math.min(totalPages, start + maxVisible);

        if (end - start < maxVisible) {
            start = Math.max(0, end - maxVisible);
        }

        for (let i = start; i < end; i++) {
            pages.push(i);
        }

        return pages;
    }

    openCreateModal(): void {
        this.isEditMode = false;
        this.currentUserId = null;
        this.formError = null;
        this.successMessage = null;

        this.userForm.reset({
            isActive: true,
            userName: '',
            email: '',
            fullName: '',
            password: '',
            confirmPassword: ''
        });

        // Password is required for creation with strong validation
        this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(8), strongPasswordValidator]);
        this.userForm.get('confirmPassword')?.setValidators([Validators.required]);
        this.userForm.get('password')?.updateValueAndValidity();
        this.userForm.get('confirmPassword')?.updateValueAndValidity();

        this.showModal = true;
    }

    openEditModal(user: ApplicationUserResponse): void {
        this.isEditMode = true;
        this.currentUserId = user.id;
        this.formError = null;
        this.successMessage = null;

        this.userForm.patchValue({
            userName: user.userName,
            email: user.email,
            fullName: user.fullName,
            isActive: user.isActive,
            password: '',
            confirmPassword: ''
        });

        // Password is optional for updates, but if provided must be strong
        this.userForm.get('password')?.setValidators([Validators.minLength(8), strongPasswordValidator]);
        this.userForm.get('confirmPassword')?.clearValidators();
        this.userForm.get('password')?.updateValueAndValidity();
        this.userForm.get('confirmPassword')?.updateValueAndValidity();

        this.showModal = true;
    }

    closeModal(): void {
        this.showModal = false;
        this.userForm.reset();
        this.currentUserId = null;
        this.formError = null;
        this.successMessage = null;
        this.showPassword = false;
        this.showConfirmPassword = false;
        this.passwordStrength = null;
    }

    submitForm(): void {
        if (this.userForm.invalid) {
            this.userForm.markAllAsTouched();

            // Check for password mismatch
            if (this.userForm.hasError('passwordMismatch')) {
                this.formError = 'Passwords do not match';
            } else {
                this.formError = 'Please fill in all required fields correctly';
            }
            return;
        }

        const password = this.userForm.get('password')?.value;
        const confirmPassword = this.userForm.get('confirmPassword')?.value;

        if (password && password !== confirmPassword) {
            this.formError = 'Passwords do not match';
            return;
        }

        this.isSaving = true;
        this.formError = null;
        this.successMessage = null;

        const formValue = this.userForm.value;

        // For updates, only include password if it was provided and is not empty
        const payload = this.isEditMode
            ? {
                userName: formValue.userName?.trim(),
                email: formValue.email?.trim(),
                fullName: formValue.fullName?.trim(),
                isActive: formValue.isActive,
                // Only include password if user entered one
                ...(password && password.trim() ? { password: password.trim() } : {})
            } as ApplicationUserUpdateRequest
            : {
                userName: formValue.userName?.trim(),
                email: formValue.email?.trim(),
                fullName: formValue.fullName?.trim(),
                isActive: formValue.isActive,
                password: password?.trim()
            } as ApplicationUserInsertRequest;

        const request$ = this.isEditMode && this.currentUserId
            ? this.userService.update(this.currentUserId, payload as ApplicationUserUpdateRequest)
            : this.userService.create(payload as ApplicationUserInsertRequest);

        request$
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isSaving = false)
            )
            .subscribe({
                next: () => {
                    this.successMessage = this.isEditMode
                        ? 'User updated successfully'
                        : 'User created successfully';

                    // Close modal after short delay to show success message
                    setTimeout(() => {
                        this.closeModal();
                        this.loadUsers();
                    }, 1500);
                },
                error: (error) => {
                    this.formError = error.message || 'An error occurred while saving';
                    console.error('Save failed:', error);
                }
            });
    }

    calculatePasswordStrength(password: string): void {
        if (!password) {
            this.passwordStrength = null;
            return;
        }

        let strength = 0;

        // Length check
        if (password.length >= 8) strength++;
        if (password.length >= 12) strength++;

        // Character variety checks
        if (/[a-z]/.test(password)) strength++;
        if (/[A-Z]/.test(password)) strength++;
        if (/[0-9]/.test(password)) strength++;
        if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) strength++;

        // Set strength level
        if (strength <= 2) {
            this.passwordStrength = 'weak';
        } else if (strength <= 4) {
            this.passwordStrength = 'medium';
        } else {
            this.passwordStrength = 'strong';
        }
    }

    togglePasswordVisibility(): void {
        this.showPassword = !this.showPassword;
    }

    toggleConfirmPasswordVisibility(): void {
        this.showConfirmPassword = !this.showConfirmPassword;
    }

    getPasswordError(): string | null {
        const passwordControl = this.userForm.get('password');
        if (!passwordControl?.touched || !passwordControl?.errors) return null;

        if (passwordControl.errors['required']) {
            return 'Password is required';
        }
        if (passwordControl.errors['minlength']) {
            return 'Password must be at least 8 characters';
        }
        if (passwordControl.errors['weakPassword']) {
            return 'Password must contain uppercase, lowercase, number, and special character';
        }
        return null;
    }

    isFieldInvalid(fieldName: string): boolean {
        const field = this.userForm.get(fieldName);
        return !!(field && field.invalid && field.touched);
    }

    hasPasswordMismatch(): boolean {
        const password = this.userForm.get('password');
        const confirmPassword = this.userForm.get('confirmPassword');

        return !!(
            password?.value &&
            confirmPassword?.value &&
            password.value !== confirmPassword.value &&
            confirmPassword.touched
        );
    }

    confirmDelete(user: ApplicationUserResponse): void {
        this.userToDelete = user;
        this.showDeleteModal = true;
    }

    cancelDelete(): void {
        this.userToDelete = null;
        this.showDeleteModal = false;
    }

    executeDelete(): void {
        if (!this.userToDelete) return;

        this.isDeleting = true;

        this.userService.delete(this.userToDelete.id)
            .pipe(
                takeUntil(this.destroy$),
                finalize(() => this.isDeleting = false)
            )
            .subscribe({
                next: () => {
                    this.cancelDelete();
                    this.loadUsers();
                },
                error: (error) => {
                    console.error('Delete failed:', error);
                    this.formError = error.message || 'Failed to delete user';
                }
            });
    }



    formatDate(dateString: string | undefined): string {
        if (!dateString) return 'Never';
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    getInitials(fullName: string): string {
        if (!fullName) return '??';
        return fullName
            .split(' ')
            .filter(n => n.length > 0)
            .map(n => n[0])
            .join('')
            .toUpperCase()
            .substring(0, 2);
    }
}