export const STORAGE_KEY = 'lwt-theme';

// Runs before the page paints, so the saved (or system) theme applies without a flash.
export const themeScript = `(function(){try{var t=localStorage.getItem('${STORAGE_KEY}');var d=t==='dark'||((!t||t==='system')&&matchMedia('(prefers-color-scheme: dark)').matches);document.documentElement.classList.toggle('dark',d)}catch(e){}})()`;
