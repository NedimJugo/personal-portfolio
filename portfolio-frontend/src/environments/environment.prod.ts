export const environment = {
  production: true,
  apiUrl: 'https://your-production-api.com/api',
  apiEndpoints: {
    auth: {
      login: '/auth/login',
      refresh: '/auth/refresh',
      logout: '/auth/logout'
    },
    projects: '/projects',
    admin: '/admin'
  }
};