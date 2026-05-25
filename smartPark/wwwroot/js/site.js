// SmartPark - pomocne JS funkcije
function spConfirm(msg) { return window.confirm(msg || 'Da li ste sigurni?'); }
function spToggleSidebar() {
  var s = document.getElementById('parkingSidebar');
  if (s) s.classList.toggle('d-none');
}
