// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
const CACHE_VERSION = 20241115.1;
self.addEventListener('fetch', () => { });
