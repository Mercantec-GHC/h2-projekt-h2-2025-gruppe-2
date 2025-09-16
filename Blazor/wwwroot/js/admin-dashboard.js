window.initAdminDashboard = () => {
  try {
    if (window.feather && typeof window.feather.replace === 'function') {
      window.feather.replace();
    }
    if (window.AOS && typeof window.AOS.init === 'function') {
      window.AOS.init();
    }

    // If Chart.js is later added, you can initialize placeholder charts here.
    const revenue = document.getElementById('revenueChart');
    const occupancy = document.getElementById('occupancyChart');
    if (revenue && occupancy && window.Chart) {
      // Example setup can go here
    }
  } catch (e) {
    console.error('Failed to init admin dashboard', e);
  }
};
