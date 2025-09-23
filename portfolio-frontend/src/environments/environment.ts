export const environment = {
  production: false,
  apiUrl: 'http://localhost:7001/api',
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