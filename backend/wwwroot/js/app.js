/* STRIDE Static Storefront (no backend) */

const PRODUCTS = [
  {
    id: "nova-runner",
    name: "Nova Runner",
    category: "running",
    price: 129,
    rating: 4.9,
    tags: ["Best seller"],
    isNew: true,
    colorway: "Violet / Ice",
    img: "assets/products/nova-runner.svg",
    desc: "Featherweight bounce with a stable heel—built for daily miles and post-run coffee.",
    sizes: [6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5, 10, 10.5, 11, 12],
    createdAt: "2026-04-12",
  },
  {
    id: "city-studio-low",
    name: "City Studio Low",
    category: "lifestyle",
    price: 139,
    rating: 4.8,
    tags: ["Premium"],
    isNew: false,
    colorway: "Chalk / Sand",
    img: "assets/products/city-studio-low.svg",
    desc: "Clean leather lines with soft padding. Dress it up or down—always comfortable.",
    sizes: [6, 7, 7.5, 8, 8.5, 9, 9.5, 10, 11, 12],
    createdAt: "2026-02-10",
  },
  {
    id: "pulse-trainer",
    name: "Pulse Trainer",
    category: "training",
    price: 119,
    rating: 4.7,
    tags: ["Gym"],
    isNew: true,
    colorway: "Graphite / Neon",
    img: "assets/products/pulse-trainer.svg",
    desc: "Wide base + responsive foam for lifting, HIIT, and all-day steps.",
    sizes: [6, 6.5, 7, 7.5, 8, 8.5, 9, 9.5, 10, 10.5, 11],
    createdAt: "2026-03-22",
  },
  {
    id: "trailworks-pro",
    name: "Trailworks Pro",
    category: "outdoor",
    price: 159,
    rating: 4.8,
    tags: ["Trail grip"],
    isNew: false,
    colorway: "Olive / Stone",
    img: "assets/products/trailworks-pro.svg",
    desc: "Rugged upper, deep lugs, and a secure midfoot wrap for uneven terrain.",
    sizes: [7, 7.5, 8, 8.5, 9, 9.5, 10, 10.5, 11, 12],
    createdAt: "2025-12-18",
  },
  {
    id: "aero-knit",
    name: "Aero Knit",
    category: "running",
    price: 149,
    rating: 4.9,
    tags: ["Max cushion"],
    isNew: false,
    colorway: "Midnight / Mist",
    img: "assets/products/aero-knit.svg",
    desc: "Soft landings with a smooth rocker—perfect for long runs and travel days.",
    sizes: [6, 6.5, 7, 8, 8.5, 9, 9.5, 10, 10.5, 11, 12],
    createdAt: "2026-01-30",
  },
  {
    id: "heritage-court",
    name: "Heritage Court",
    category: "lifestyle",
    price: 109,
    rating: 4.6,
    tags: ["Classic"],
    isNew: false,
    colorway: "White / Navy",
    img: "assets/products/heritage-court.svg",
    desc: "Vintage-inspired profile with modern cushioning—your go-to everyday pair.",
    sizes: [6, 7, 8, 8.5, 9, 9.5, 10, 11, 12],
    createdAt: "2025-11-08",
  },
  {
    id: "summit-hike",
    name: "Summit Hike",
    category: "outdoor",
    price: 179,
    rating: 4.7,
    tags: ["Water resistant"],
    isNew: true,
    colorway: "Black / Ember",
    img: "assets/products/summit-hike.svg",
    desc: "Weather-ready comfort with a supportive collar and confident traction.",
    sizes: [7, 7.5, 8, 9, 9.5, 10, 10.5, 11, 12],
    createdAt: "2026-05-02",
  },
  {
    id: "flex-shift",
    name: "Flex Shift",
    category: "training",
    price: 99,
    rating: 4.5,
    tags: ["Light"],
    isNew: false,
    colorway: "Slate / Lime",
    img: "assets/products/flex-shift.svg",
    desc: "Ultra-flex grooves and breathable mesh—great for classes and commuting.",
    sizes: [6, 6.5, 7, 7.5, 8, 9, 9.5, 10, 10.5, 11, 12],
    createdAt: "2025-10-28",
  },
  {
    id: "drift-slide",
    name: "Drift Slide",
    category: "lifestyle",
    price: 65,
    rating: 4.4,
    tags: ["Recovery"],
    isNew: true,
    colorway: "Cloud / Lilac",
    img: "assets/products/drift-slide.svg",
    desc: "Soft recovery foam with a stable cradle—post-workout comfort, all day.",
    sizes: [6, 7, 8, 9, 10, 11, 12],
    createdAt: "2026-04-28",
  },
];

const $ = (sel, root = document) => root.querySelector(sel);
const $$ = (sel, root = document) => Array.from(root.querySelectorAll(sel));

const formatUSD = (n) =>
  new Intl.NumberFormat(undefined, { style: "currency", currency: "USD", maximumFractionDigits: 0 }).format(n);

const clamp = (v, a, b) => Math.min(b, Math.max(a, v));

const STORAGE_KEYS = {
  cart: "stride_cart_v1",
  theme: "stride_theme_v1",
  auth: "stride_auth_v1",
};

function safeJSONParse(value, fallback) {
  try {
    return JSON.parse(value);
  } catch {
    return fallback;
  }
}

function loadCart() {
  const raw = localStorage.getItem(STORAGE_KEYS.cart);
  const data = safeJSONParse(raw, null);
  if (!data || typeof data !== "object") return [];
  if (!Array.isArray(data.items)) return [];
  return data.items.filter((x) => x && typeof x.id === "string" && typeof x.qty === "number");
}

function saveCart(items) {
  localStorage.setItem(STORAGE_KEYS.cart, JSON.stringify({ items }));
}

function getTheme() {
  const stored = localStorage.getItem(STORAGE_KEYS.theme);
  if (stored === "light" || stored === "dark") return stored;
  return window.matchMedia && window.matchMedia("(prefers-color-scheme: light)").matches ? "light" : "dark";
}

function setTheme(theme) {
  document.documentElement.dataset.theme = theme;
  localStorage.setItem(STORAGE_KEYS.theme, theme);
}

function loadAuthToken() {
  const raw = localStorage.getItem(STORAGE_KEYS.auth);
  const data = safeJSONParse(raw, null);
  if (!data || typeof data !== "object") return null;
  return typeof data.token === "string" && data.token.trim() ? data.token : null;
}

function saveAuthToken(token) {
  if (!token) localStorage.removeItem(STORAGE_KEYS.auth);
  else localStorage.setItem(STORAGE_KEYS.auth, JSON.stringify({ token }));
}

const state = {
  category: "all",
  maxPrice: 220,
  query: "",
  sort: "featured",
  cart: loadCart(),
  modalProductId: null,
  modalSize: null,
  auth: {
    mode: "login", // login | register
    token: loadAuthToken(),
    me: null,
  },
};

async function apiRequest(path, { method = "GET", body, auth = true } = {}) {
  const headers = { Accept: "application/json" };
  if (body !== undefined) headers["Content-Type"] = "application/json";
  if (auth && state.auth.token) headers.Authorization = `Bearer ${state.auth.token}`;

  const res = await fetch(path, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  const contentType = res.headers.get("content-type") || "";
  const isJson = contentType.toLowerCase().includes("application/json");
  const payload = isJson ? await res.json().catch(() => null) : await res.text().catch(() => "");

  if (!res.ok) {
    const message =
      payload && typeof payload === "object" && (payload.message || payload.title)
        ? payload.message || payload.title
        : `HTTP ${res.status}`;
    const err = new Error(message);
    err.status = res.status;
    throw err;
  }

  return payload;
}

function productById(id) {
  return PRODUCTS.find((p) => p.id === id) || null;
}

function cartCount(items = state.cart) {
  return items.reduce((sum, it) => sum + it.qty, 0);
}

function cartSubtotal(items = state.cart) {
  return items.reduce((sum, it) => {
    const p = productById(it.id);
    return p ? sum + p.price * it.qty : sum;
  }, 0);
}

function matchesFilters(p) {
  const inCategory = state.category === "all" ? true : p.category === state.category;
  const inPrice = p.price <= state.maxPrice;
  const q = state.query.trim().toLowerCase();
  const inQuery =
    q.length === 0
      ? true
      : `${p.name} ${p.category} ${p.colorway} ${p.tags.join(" ")} ${p.desc}`.toLowerCase().includes(q);
  return inCategory && inPrice && inQuery;
}

function sortProducts(list) {
  const copy = [...list];
  const sort = state.sort;

  if (sort === "price_asc") return copy.sort((a, b) => a.price - b.price);
  if (sort === "price_desc") return copy.sort((a, b) => b.price - a.price);
  if (sort === "rating_desc") return copy.sort((a, b) => b.rating - a.rating);
  if (sort === "newest") return copy.sort((a, b) => +new Date(b.createdAt) - +new Date(a.createdAt));

  // featured: new first, then rating, then price mid
  return copy.sort((a, b) => {
    const an = a.isNew ? 1 : 0;
    const bn = b.isNew ? 1 : 0;
    if (an !== bn) return bn - an;
    if (a.rating !== b.rating) return b.rating - a.rating;
    return Math.abs(135 - a.price) - Math.abs(135 - b.price);
  });
}

function stars(rating) {
  const full = Math.round(clamp(rating, 0, 5));
  return "★★★★★".slice(0, full) + "☆☆☆☆☆".slice(full);
}

function renderProducts() {
  const grid = $('[data-bind="productGrid"]');
  const empty = $('[data-bind="emptyState"]');
  if (!grid || !empty) return;

  const filtered = sortProducts(PRODUCTS.filter(matchesFilters));

  grid.innerHTML = filtered
    .map((p) => {
      const tags = [
        p.isNew ? `<span class="tag new">New</span>` : "",
        p.tags.includes("Best seller") ? `<span class="tag hot">Best</span>` : "",
      ]
        .filter(Boolean)
        .join("");

      return `
        <article class="card" aria-label="${escapeHtml(p.name)}">
          <div class="card-media">
            <img src="${p.img}" alt="${escapeHtml(p.name)} shoe" loading="lazy" width="520" height="340" />
            <div class="card-badges">${tags}</div>
          </div>
          <div class="card-body">
            <div class="card-title-row">
              <div class="card-title">${escapeHtml(p.name)}</div>
              <div class="card-price">${formatUSD(p.price)}</div>
            </div>
            <div class="card-meta">
              <span>${escapeHtml(titleCase(p.category))} • ${escapeHtml(p.colorway)}</span>
              <span class="stars" aria-label="Rating ${p.rating} out of 5">${stars(p.rating)}</span>
            </div>
            <div class="card-actions">
              <button class="btn small" type="button" data-action="quick-view" data-id="${p.id}">Quick view</button>
              <button class="btn small primary" type="button" data-action="add-to-cart" data-id="${p.id}">
                Add
              </button>
            </div>
          </div>
        </article>
      `;
    })
    .join("");

  empty.hidden = filtered.length !== 0;
}

function renderCart() {
  const container = $('[data-bind="cartItems"]');
  const subtotalEl = $('[data-bind="cartSubtotal"]');
  const countEl = $('[data-bind="cartCount"]');
  if (!container || !subtotalEl || !countEl) return;

  countEl.textContent = String(cartCount());
  subtotalEl.textContent = formatUSD(cartSubtotal());

  if (state.cart.length === 0) {
    container.innerHTML = `
      <div class="empty-state">
        <div class="empty-title">Cart is empty</div>
        <p class="empty-text">Add a pair from the shop—your cart saves to this device.</p>
        <button class="btn" type="button" data-action="close-cart">Continue shopping</button>
      </div>
    `;
    return;
  }

  container.innerHTML = state.cart
    .map((it) => {
      const p = productById(it.id);
      if (!p) return "";
      const key = `${it.id}__${it.size || ""}`;
      return `
        <div class="cart-item" data-key="${key}">
          <img src="${p.img}" alt="${escapeHtml(p.name)} shoe" loading="lazy" width="220" height="140" />
          <div>
            <div class="cart-item-title">${escapeHtml(p.name)}</div>
            <div class="cart-item-sub">${escapeHtml(it.size ? `Size ${it.size} • ` : "")}${escapeHtml(
        p.colorway
      )}</div>
            <div class="cart-item-sub">${formatUSD(p.price)} each</div>
          </div>
          <div class="qty" aria-label="Quantity">
            <button type="button" data-action="dec-qty" aria-label="Decrease quantity">−</button>
            <div class="qty-value" aria-label="Quantity value">${it.qty}</div>
            <button type="button" data-action="inc-qty" aria-label="Increase quantity">+</button>
          </div>
        </div>
      `;
    })
    .join("");
}

function openDrawer(el) {
  if (!el) return;
  el.classList.add("is-open");
  el.setAttribute("aria-hidden", "false");
  if (el.id === "cartDrawer") {
    const btn = $('[data-action="open-cart"]');
    if (btn) btn.setAttribute("aria-expanded", "true");
  }
  openScrim();
}

function closeDrawer(el) {
  if (!el) return;
  el.classList.remove("is-open");
  el.setAttribute("aria-hidden", "true");
  if (el.id === "cartDrawer") {
    const btn = $('[data-action="open-cart"]');
    if (btn) btn.setAttribute("aria-expanded", "false");
  }
  maybeCloseScrim();
}

function openSidebar() {
  const sidebar = $("#sidebar");
  if (!sidebar) return;
  sidebar.classList.add("is-open");
  sidebar.setAttribute("aria-hidden", "false");
  const btn = $('[data-action="open-sidebar"]');
  if (btn) btn.setAttribute("aria-expanded", "true");
  openScrim();
}

function closeSidebar() {
  const sidebar = $("#sidebar");
  if (!sidebar) return;
  sidebar.classList.remove("is-open");
  sidebar.setAttribute("aria-hidden", "true");
  const btn = $('[data-action="open-sidebar"]');
  if (btn) btn.setAttribute("aria-expanded", "false");
  maybeCloseScrim();
}

function openScrim() {
  const scrim = $(".scrim");
  if (!scrim) return;
  scrim.classList.add("is-open");
  scrim.setAttribute("aria-hidden", "false");
}

function maybeCloseScrim() {
  const sidebar = $("#sidebar");
  const drawer = $("#cartDrawer");
  const modal = $('[data-bind="productModal"]');
  const anyOpen =
    (sidebar && sidebar.classList.contains("is-open")) ||
    (drawer && drawer.classList.contains("is-open")) ||
    (modal && typeof modal.open === "boolean" && modal.open);
  if (!anyOpen) {
    const scrim = $(".scrim");
    if (!scrim) return;
    scrim.classList.remove("is-open");
    scrim.setAttribute("aria-hidden", "true");
  }
}

function toast(msg) {
  const t = $('[data-bind="toast"]');
  if (!t) return;
  t.textContent = msg;
  t.hidden = false;
  clearTimeout(toast._timer);
  toast._timer = setTimeout(() => {
    t.hidden = true;
  }, 1800);
}

function addToCart(productId, { size } = {}) {
  const p = productById(productId);
  if (!p) return;

  const key = `${productId}__${size || ""}`;
  const existing = state.cart.find((it) => `${it.id}__${it.size || ""}` === key);
  if (existing) existing.qty += 1;
  else state.cart.push({ id: productId, qty: 1, size: size || null });

  saveCart(state.cart);
  renderCart();
  toast("Added to cart");
}

function setCategory(category, { scroll = true } = {}) {
  state.category = category;
  // sync chips
  $$('.chip[data-category]').forEach((c) => c.classList.toggle("is-active", c.dataset.category === category));
  // sync sidebar pills (simple highlight via aria-pressed)
  $$('.pill[data-category]').forEach((p) => p.setAttribute("aria-pressed", String(p.dataset.category === category)));
  renderProducts();
  if (scroll) {
    const shop = $("#shop");
    if (shop) shop.scrollIntoView({ behavior: "smooth", block: "start" });
  }
}

function resetFilters() {
  state.category = "all";
  state.query = "";
  state.maxPrice = 220;
  state.sort = "featured";
  $$('.chip[data-category]').forEach((c) => c.classList.toggle("is-active", c.dataset.category === "all"));
  $$('[data-bind="query"]').forEach((el) => (el.value = ""));
  const range = $('[data-bind="maxPrice"]');
  const label = $('[data-bind="maxPriceLabel"]');
  if (range) range.value = String(state.maxPrice);
  if (label) label.textContent = formatUSD(state.maxPrice);
  const sort = $('[data-bind="sort"]');
  if (sort) sort.value = state.sort;
  renderProducts();
}

function openProductModal(productId) {
  const modal = $('[data-bind="productModal"]');
  if (!modal) return;
  const p = productById(productId);
  if (!p) return;

  state.modalProductId = p.id;
  state.modalSize = p.sizes.includes(9) ? 9 : p.sizes[0] || null;

  $('[data-bind="modalTitle"]').textContent = p.name;
  $('[data-bind="modalPrice"]').textContent = formatUSD(p.price);
  $('[data-bind="modalRating"]').textContent = `${stars(p.rating)}  (${p.rating.toFixed(1)})`;
  $('[data-bind="modalDesc"]').textContent = p.desc;

  const media = $('[data-bind="modalMedia"]');
  media.innerHTML = `<img src="${p.img}" alt="${escapeHtml(p.name)} shoe" width="720" height="460" />`;

  const grid = $('[data-bind="sizeGrid"]');
  grid.innerHTML = p.sizes
    .map((s) => {
      const pressed = s === state.modalSize ? "true" : "false";
      return `<button class="size-btn" type="button" aria-pressed="${pressed}" data-action="pick-size" data-size="${s}">${s}</button>`;
    })
    .join("");

  openScrim();
  if (!modal.open) modal.showModal();
}

function closeProductModal() {
  const modal = $('[data-bind="productModal"]');
  if (!modal) return;
  if (modal.open) modal.close();
  maybeCloseScrim();
}

function openAuthModal() {
  const modal = $('[data-bind="authModal"]');
  if (!modal) return;
  renderAuth();
  if (!modal.open) modal.showModal();
}

function closeAuthModal() {
  const modal = $('[data-bind="authModal"]');
  if (!modal) return;
  if (modal.open) modal.close();
}

function setAuthMode(mode) {
  state.auth.mode = mode === "register" ? "register" : "login";
  setAuthError(null);
  renderAuth();
}

function setAuthError(message) {
  const el = $('[data-bind="authError"]');
  if (!el) return;
  if (!message) {
    el.hidden = true;
    el.textContent = "";
    return;
  }
  el.hidden = false;
  el.textContent = message;
}

function clearAuth() {
  state.auth.token = null;
  state.auth.me = null;
  saveAuthToken(null);
  renderAuth();
}

async function initAuth() {
  renderAuth();

  if (!state.auth.token) return;
  try {
    state.auth.me = await apiRequest("/api/auth/me");
  } catch {
    clearAuth();
  }

  renderAuth();
}

function renderAuth() {
  const chip = $('[data-bind="authChip"]');
  if (chip) {
    if (state.auth.me) {
      chip.hidden = false;
      chip.textContent = state.auth.me.username;
      chip.title = state.auth.me.email || "Signed in";
    } else {
      chip.hidden = true;
      chip.textContent = "";
      chip.title = "";
    }
  }

  const loggedIn = $('[data-bind="authLoggedIn"]');
  const loggedOut = $('[data-bind="authLoggedOut"]');
  const isSignedIn = Boolean(state.auth.me);

  if (loggedIn) loggedIn.hidden = !isSignedIn;
  if (loggedOut) loggedOut.hidden = isSignedIn;

  const userEl = $('[data-bind="authUser"]');
  if (userEl) {
    userEl.textContent = state.auth.me ? `${state.auth.me.username} (${state.auth.me.email})` : "";
  }

  const loginForm = $('[data-bind="authLoginForm"]');
  const registerForm = $('[data-bind="authRegisterForm"]');
  if (loginForm) loginForm.hidden = state.auth.mode !== "login";
  if (registerForm) registerForm.hidden = state.auth.mode !== "register";

  $$('.auth-tab[data-action="auth-set-mode"]').forEach((btn) => {
    btn.setAttribute("aria-pressed", String(btn.dataset.mode === state.auth.mode));
  });
}

async function authLogin() {
  setAuthError(null);
  const identifier = $("#authIdentifier")?.value?.trim() || "";
  const password = $("#authPassword")?.value || "";
  if (!identifier || !password) {
    setAuthError("Enter your email/username and password.");
    return;
  }

  try {
    const res = await apiRequest("/api/auth/login", {
      method: "POST",
      body: { identifier, password },
      auth: false,
    });

    state.auth.token = res.accessToken;
    saveAuthToken(res.accessToken);
    state.auth.me = await apiRequest("/api/auth/me");
    setAuthError(null);
    renderAuth();
    toast(`Welcome back, ${state.auth.me.username}`);
  } catch (e) {
    setAuthError(e?.message || "Unable to sign in.");
  }
}

async function authRegister() {
  setAuthError(null);
  const username = $("#authUsername")?.value?.trim() || "";
  const email = $("#authEmail")?.value?.trim() || "";
  const password = $("#authNewPassword")?.value || "";

  if (!username || !email || !password) {
    setAuthError("Fill in username, email, and password.");
    return;
  }

  try {
    const res = await apiRequest("/api/auth/register", {
      method: "POST",
      body: { username, email, password },
      auth: false,
    });

    state.auth.token = res.accessToken;
    saveAuthToken(res.accessToken);
    state.auth.me = await apiRequest("/api/auth/me");
    setAuthError(null);
    renderAuth();
    toast(`Account created: ${state.auth.me.username}`);
  } catch (e) {
    setAuthError(e?.message || "Unable to create account.");
  }
}

function logout() {
  clearAuth();
  toast("Signed out");
}

async function checkout() {
  if (state.cart.length === 0) {
    toast("Cart is empty");
    return;
  }

  if (!state.auth.token || !state.auth.me) {
    toast("Sign in to checkout");
    openAuthModal();
    return;
  }

  const items = state.cart
    .map((it) => {
      const p = productById(it.id);
      if (!p) return null;
      return {
        productId: p.id,
        name: p.name,
        quantity: it.qty,
        unitPrice: p.price,
        size: it.size ? String(it.size) : null,
      };
    })
    .filter(Boolean);

  try {
    const purchase = await apiRequest("/api/purchases", { method: "POST", body: { items } });

    state.cart = [];
    saveCart(state.cart);
    renderCart();
    closeDrawer($("#cartDrawer"));

    const id = purchase?.id ? String(purchase.id).slice(0, 8) : "";
    toast(id ? `Purchase saved (#${id})` : "Purchase saved");
  } catch (e) {
    if (e?.status === 401) {
      clearAuth();
      toast("Session expired â€” sign in again");
      openAuthModal();
      return;
    }

    toast(e?.message || "Checkout failed");
  }
}

function escapeHtml(str) {
  return String(str)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

function titleCase(str) {
  return String(str).slice(0, 1).toUpperCase() + String(str).slice(1);
}

function syncURLFromState() {
  const params = new URLSearchParams(window.location.search);
  if (state.category !== "all") params.set("cat", state.category);
  else params.delete("cat");

  const q = state.query.trim();
  if (q) params.set("q", q);
  else params.delete("q");

  if (state.maxPrice !== 220) params.set("max", String(state.maxPrice));
  else params.delete("max");

  if (state.sort !== "featured") params.set("sort", state.sort);
  else params.delete("sort");

  const url = `${window.location.pathname}${params.toString() ? `?${params.toString()}` : ""}${window.location.hash}`;
  window.history.replaceState(null, "", url);
}

function hydrateStateFromURL() {
  const params = new URLSearchParams(window.location.search);
  const cat = params.get("cat");
  const q = params.get("q");
  const max = params.get("max");
  const sort = params.get("sort");

  if (cat && ["all", "running", "lifestyle", "training", "outdoor"].includes(cat)) state.category = cat;
  if (typeof q === "string") state.query = q.slice(0, 80);
  if (max && Number.isFinite(+max)) state.maxPrice = clamp(+max, 60, 220);
  if (sort && ["featured", "price_asc", "price_desc", "rating_desc", "newest"].includes(sort)) state.sort = sort;
}

function bindControls() {
  // Year
  const year = $('[data-bind="year"]');
  if (year) year.textContent = String(new Date().getFullYear());

  // Inputs bound to query
  $$('[data-bind="query"]').forEach((el) => {
    el.value = state.query;
    el.addEventListener("input", () => {
      state.query = el.value;
      // keep nav + shop in sync
      $$('[data-bind="query"]').forEach((e2) => {
        if (e2 !== el) e2.value = el.value;
      });
      renderProducts();
      syncURLFromState();
    });
  });

  // Prevent full-page reload on Enter in navbar search
  $$(".nav-search").forEach((form) => {
    form.addEventListener("submit", (e) => e.preventDefault());
  });

  // Max price
  const maxPrice = $('[data-bind="maxPrice"]');
  const maxLabel = $('[data-bind="maxPriceLabel"]');
  if (maxPrice && maxLabel) {
    maxPrice.value = String(state.maxPrice);
    maxLabel.textContent = formatUSD(state.maxPrice);
    maxPrice.addEventListener("input", () => {
      state.maxPrice = +maxPrice.value;
      maxLabel.textContent = formatUSD(state.maxPrice);
      renderProducts();
      syncURLFromState();
    });
  }

  // Sort
  const sort = $('[data-bind="sort"]');
  if (sort) {
    sort.value = state.sort;
    sort.addEventListener("change", () => {
      state.sort = sort.value;
      renderProducts();
      syncURLFromState();
    });
  }

  // Category chips initial
  $$('.chip[data-category]').forEach((c) => c.classList.toggle("is-active", c.dataset.category === state.category));
  $$('.pill[data-category]').forEach((p) => p.setAttribute("aria-pressed", String(p.dataset.category === state.category)));

  // Newsletter (demo)
  const newsletter = $('[data-action="newsletter"]');
  if (newsletter) {
    newsletter.addEventListener("submit", (e) => {
      e.preventDefault();
      const email = $("#newsletterEmail")?.value?.trim() || "";
      $("#newsletterEmail").value = "";
      toast(email ? "Thanks — check your inbox (demo)" : "Enter an email");
    });
  }
}

function handleGlobalClick(e) {
  const t = e.target;
  if (!(t instanceof Element)) return;

  const actionEl = t.closest("[data-action]");
  if (!actionEl) return;
  const action = actionEl.dataset.action;

  if (action === "open-sidebar") return openSidebar();
  if (action === "close-sidebar") return closeSidebar();

  if (action === "open-cart") return openDrawer($("#cartDrawer"));
  if (action === "close-cart") return closeDrawer($("#cartDrawer"));

  if (action === "close-overlays") {
    closeSidebar();
    closeDrawer($("#cartDrawer"));
    closeProductModal();
    return;
  }

  if (action === "toggle-theme") {
    const next = document.documentElement.dataset.theme === "light" ? "dark" : "light";
    setTheme(next);
    toast(`Theme: ${next}`);
    return;
  }

  if (action === "open-auth") return openAuthModal();
  if (action === "close-auth") return closeAuthModal();
  if (action === "auth-set-mode") return setAuthMode(actionEl.dataset.mode);
  if (action === "auth-login") return void authLogin();
  if (action === "auth-register") return void authRegister();
  if (action === "logout") return logout();

  if (action === "set-category") {
    const cat = actionEl.dataset.category || "all";
    setCategory(cat, { scroll: true });
    closeSidebar();
    syncURLFromState();
    return;
  }

  if (action === "reset-filters") {
    resetFilters();
    syncURLFromState();
    return;
  }

  if (action === "add-to-cart") {
    const id = actionEl.dataset.id;
    addToCart(id);
    renderCart();
    return;
  }

  if (action === "quick-view") {
    const id = actionEl.dataset.id;
    openProductModal(id);
    return;
  }

  if (action === "pick-size") {
    const size = Number(actionEl.dataset.size);
    state.modalSize = Number.isFinite(size) ? size : state.modalSize;
    // update pressed states
    $$('.size-btn[data-action="pick-size"]').forEach((b) => {
      b.setAttribute("aria-pressed", String(Number(b.dataset.size) === state.modalSize));
    });
    return;
  }

  if (action === "add-to-cart-from-modal") {
    if (!state.modalProductId) return;
    addToCart(state.modalProductId, { size: state.modalSize });
    renderCart();
    openDrawer($("#cartDrawer"));
    closeProductModal();
    return;
  }

  if (action === "copy-link") {
    const url = new URL(window.location.href);
    url.search = new URLSearchParams({
      ...(state.category !== "all" ? { cat: state.category } : {}),
      ...(state.query.trim() ? { q: state.query.trim() } : {}),
      ...(state.maxPrice !== 220 ? { max: String(state.maxPrice) } : {}),
      ...(state.sort !== "featured" ? { sort: state.sort } : {}),
    }).toString();
    url.hash = "#shop";
    navigator.clipboard?.writeText(url.href).then(
      () => toast("Link copied"),
      () => toast("Copy not available")
    );
    return;
  }

  if (action === "checkout") {
    checkout();
    return;
  }

  if (action === "scroll-to-cart") {
    openDrawer($("#cartDrawer"));
    return;
  }

  if (action === "inc-qty" || action === "dec-qty") {
    const itemEl = actionEl.closest(".cart-item");
    const key = itemEl?.getAttribute("data-key");
    if (!key) return;

    const idx = state.cart.findIndex((it) => `${it.id}__${it.size || ""}` === key);
    if (idx < 0) return;

    const it = state.cart[idx];
    if (action === "inc-qty") it.qty += 1;
    else it.qty -= 1;

    if (it.qty <= 0) state.cart.splice(idx, 1);
    saveCart(state.cart);
    renderCart();
    return;
  }
}

function handleKeydown(e) {
  if (e.key === "Escape") {
    closeSidebar();
    closeDrawer($("#cartDrawer"));
    closeProductModal();
  }
}

function wireModal() {
  const modal = $('[data-bind="productModal"]');
  if (!modal) return;
  modal.addEventListener("close", () => {
    state.modalProductId = null;
    state.modalSize = null;
    maybeCloseScrim();
  });
}

function wireAuthModal() {
  const modal = $('[data-bind="authModal"]');
  if (!modal) return;
  modal.addEventListener("close", () => {
    setAuthError(null);
  });
}

function init() {
  setTheme(getTheme());
  hydrateStateFromURL();
  bindControls();
  renderProducts();
  renderCart();
  wireModal();
  wireAuthModal();
  initAuth();

  document.addEventListener("click", handleGlobalClick);
  document.addEventListener("keydown", handleKeydown);

  // Set initial state UI
  if (state.category !== "all") setCategory(state.category, { scroll: false });
  const maxLabel = $('[data-bind="maxPriceLabel"]');
  if (maxLabel) maxLabel.textContent = formatUSD(state.maxPrice);
  const sort = $('[data-bind="sort"]');
  if (sort) sort.value = state.sort;

  // Close overlays on hash navigation
  window.addEventListener("hashchange", () => {
    closeSidebar();
    closeDrawer($("#cartDrawer"));
    closeProductModal();
  });
}

init();
