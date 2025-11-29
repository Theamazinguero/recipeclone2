// ---------------- CONFIG ----------------
const API_BASE = "http://localhost:5000"; // change port if needed

// ---------------- STATE ----------------
let authToken = null;
let currentUser = null; // { email, displayName, isAdmin, userId }

// ---------------- HELPERS ----------------
function $(id) {
    return document.getElementById(id);
}

function setStatus(text) {
    $("statusText").textContent = text;
}

function setMessage(el, text, isError = false) {
    el.textContent = text || "";
    el.classList.remove("error", "success");
    if (!text) return;
    el.classList.add(isError ? "error" : "success");
}

async function apiRequest(path, options = {}) {
    const url = API_BASE + path;
    const headers = options.headers || {};
    headers["Content-Type"] = headers["Content-Type"] || "application/json";
    if (authToken) {
        headers["Authorization"] = "Bearer " + authToken;
    }

    const response = await fetch(url, {
        ...options,
        headers
    });

    let data = null;
    try {
        data = await response.json();
    } catch {
        // ignore body parse errors (204 etc.)
    }

    if (!response.ok) {
        const msg = (data && (data.message || data.title)) || response.statusText;
        throw new Error(msg || "Request failed");
    }

    return data;
}

// ---------------- AUTH UI / LOGIC ----------------
function initAuthTabs() {
    const tabLogin = $("tabLogin");
    const tabRegister = $("tabRegister");
    const loginForm = $("loginForm");
    const registerForm = $("registerForm");

    tabLogin.addEventListener("click", () => {
        tabLogin.classList.add("active");
        tabRegister.classList.remove("active");
        loginForm.classList.remove("hidden");
        registerForm.classList.add("hidden");
    });

    tabRegister.addEventListener("click", () => {
        tabRegister.classList.add("active");
        tabLogin.classList.remove("active");
        registerForm.classList.remove("hidden");
        loginForm.classList.add("hidden");
    });
}

async function handleRegister(event) {
    event.preventDefault();
    const msgEl = $("authMessage");
    setMessage(msgEl, "");

    const email = $("registerEmail").value.trim();
    const displayName = $("registerDisplayName").value.trim();
    const password = $("registerPassword").value;

    if (!email || !displayName || !password) {
        setMessage(msgEl, "Please fill all fields.", true);
        return;
    }

    try {
        const data = await apiRequest("/api/auth/register", {
            method: "POST",
            body: JSON.stringify({ email, displayName, password })
        });

        authToken = data.token;
        currentUser = {
            email: data.email,
            displayName: data.displayName,
            isAdmin: data.isAdmin,
            userId: data.userId
        };
        updateAuthUI();
        setMessage(msgEl, "Registered and logged in.", false);
        await loadRecipes();
    } catch (err) {
        setMessage(msgEl, err.message || "Register failed.", true);
    }
}

async function handleLogin(event) {
    event.preventDefault();
    const msgEl = $("authMessage");
    setMessage(msgEl, "");

    const email = $("loginEmail").value.trim();
    const password = $("loginPassword").value;

    if (!email || !password) {
        setMessage(msgEl, "Please fill both fields.", true);
        return;
    }

    try {
        const data = await apiRequest("/api/auth/login", {
            method: "POST",
            body: JSON.stringify({ email, password })
        });

        authToken = data.token;
        currentUser = {
            email: data.email,
            displayName: data.displayName,
            isAdmin: data.isAdmin,
            userId: data.userId
        };
        updateAuthUI();
        setMessage(msgEl, "Logged in successfully.", false);
        await loadRecipes();
    } catch (err) {
        setMessage(msgEl, err.message || "Login failed.", true);
    }
}

function handleLogout() {
    authToken = null;
    currentUser = null;
    updateAuthUI();
    setMessage($("authMessage"), "Logged out.");
}

function updateAuthUI() {
    const logoutBtn = $("logoutBtn");
    const authMessage = $("authMessage");
    const adminSection = $("adminSection");
    const createSection = $("createRecipeSection");

    if (currentUser) {
        setStatus(`Logged in as ${currentUser.displayName} (${currentUser.email})${currentUser.isAdmin ? " [Admin]" : ""}`);
        logoutBtn.classList.remove("hidden");
        createSection.classList.remove("hidden");
        if (currentUser.isAdmin) {
            adminSection.classList.remove("hidden");
        } else {
            adminSection.classList.add("hidden");
        }
        setMessage(authMessage, "");
    } else {
        setStatus("Not logged in");
        logoutBtn.classList.add("hidden");
        $("createRecipeSection").classList.add("hidden");
        $("adminSection").classList.add("hidden");
    }
}

// ---------------- RECIPES ----------------
function renderRecipeCard(recipe) {
    const div = document.createElement("div");
    div.className = "recipe-card";

    const createdAt = recipe.createdAtUtc ? new Date(recipe.createdAtUtc) : null;
    const dateStr = createdAt ? createdAt.toLocaleDateString() : "";

    div.innerHTML = `
        <h3>${recipe.title}</h3>
        <div class="recipe-meta">
            By ${recipe.createdByDisplayName || "Unknown"} 
            ${dateStr ? " · " + dateStr : ""} 
            ${recipe.isApproved ? " · Approved" : " · Pending"}
        </div>
        <div class="recipe-tags">
            ${recipe.tags && recipe.tags.length
                ? recipe.tags.map(t => `<span class="tag-pill">${t}</span>`).join(" ")
                : "<span class=\"hint\">No tags</span>"}
        </div>
        <p>${recipe.shortDescription || ""}</p>
    `;

    return div;
}

async function loadRecipes(searchText) {
    const listEl = $("recipesList");
    listEl.innerHTML = "Loading recipes...";

    let path = "/api/recipes";
    if (searchText && searchText.trim().length > 0) {
        const encoded = encodeURIComponent(searchText.trim());
        path += `?search=${encoded}`;
    }

    try {
        const data = await apiRequest(path, { method: "GET", headers: { "Content-Type": "application/json" } });

        listEl.innerHTML = "";
        if (!data || !data.length) {
            listEl.textContent = "No recipes found.";
            return;
        }

        data.forEach(r => {
            const card = renderRecipeCard(r);
            listEl.appendChild(card);
        });
    } catch (err) {
        listEl.textContent = "Failed to load recipes: " + err.message;
    }
}

// ---------------- CREATE RECIPE ----------------
async function handleCreateRecipe(event) {
    event.preventDefault();
    const msgEl = $("createRecipeMessage");
    setMessage(msgEl, "");

    if (!authToken || !currentUser) {
        setMessage(msgEl, "You must be logged in to create recipes.", true);
        return;
    }

    const title = $("recipeTitle").value.trim();
    const shortDescription = $("recipeShortDescription").value.trim();
    const imageUrl = $("recipeImageUrl").value.trim();
    const tagsRaw = $("recipeTags").value.trim();
    const ingredientsRaw = $("recipeIngredients").value.trim();
    const stepsRaw = $("recipeSteps").value.trim();

    if (!title) {
        setMessage(msgEl, "Title is required.", true);
        return;
    }

    const tags = tagsRaw
        ? tagsRaw.split(",").map(t => t.trim()).filter(Boolean)
        : [];

    const ingredients = ingredientsRaw
        ? ingredientsRaw.split("\n").map(line => {
            const trimmed = line.trim();
            if (!trimmed) return null;
            // naive parse: expect "quantity unit name"
            const parts = trimmed.split(" ");
            if (parts.length < 2) {
                return {
                    name: trimmed,
                    quantity: null,
                    unit: null
                };
            }
            const quantity = parts[0];
            const unit = parts[1];
            const name = parts.slice(2).join(" ") || (quantity + " " + unit);
            return { name, quantity, unit };
        }).filter(Boolean)
        : [];

    const steps = stepsRaw
        ? stepsRaw.split("\n").map((line, idx) => {
            const trimmed = line.trim();
            if (!trimmed) return null;
            return {
                stepNumber: idx + 1,
                description: trimmed
            };
        }).filter(Boolean)
        : [];

    const body = {
        title,
        shortDescription: shortDescription || null,
        imageUrl: imageUrl || null,
        instructionsSummary: null,
        tags,
        ingredients,
        steps
    };

    try {
        await apiRequest("/api/recipes", {
            method: "POST",
            body: JSON.stringify(body)
        });

        setMessage(msgEl, "Recipe submitted for approval.", false);

        // Optionally clear form
        $("createRecipeForm").reset();

    } catch (err) {
        setMessage(msgEl, "Failed to create recipe: " + (err.message || ""), true);
    }
}

// ---------------- ADMIN ----------------
function renderPendingRecipeCard(recipe) {
    const div = document.createElement("div");
    div.className = "pending-recipe-card";

    const createdAt = recipe.createdAtUtc ? new Date(recipe.createdAtUtc) : null;
    const dateStr = createdAt ? createdAt.toLocaleString() : "";

    div.innerHTML = `
        <h3>${recipe.title}</h3>
        <div class="recipe-meta">
            Author: ${recipe.author || "Unknown"} · Submitted: ${dateStr}
        </div>
        <div class="recipe-actions">
            <button data-action="approve" data-id="${recipe.id}">Approve</button>
            <button data-action="disable" data-id="${recipe.id}" class="secondary">Disable</button>
        </div>
    `;

    return div;
}

async function loadPendingRecipes() {
    const listEl = $("pendingRecipesList");
    listEl.innerHTML = "Loading pending recipes...";

    if (!currentUser || !currentUser.isAdmin) {
        listEl.textContent = "You are not an admin.";
        return;
    }

    try {
        const data = await apiRequest("/api/admin/recipes/pending", { method: "GET" });

        listEl.innerHTML = "";
        if (!data || !data.length) {
            listEl.textContent = "No pending recipes.";
            return;
        }

        data.forEach(r => {
            const card = renderPendingRecipeCard(r);
            listEl.appendChild(card);
        });

    } catch (err) {
        listEl.textContent = "Failed to load pending recipes: " + err.message;
    }
}

async function handlePendingClick(event) {
    const btn = event.target;
    if (!(btn instanceof HTMLButtonElement)) return;

    const action = btn.getAttribute("data-action");
    const id = btn.getAttribute("data-id");
    if (!action || !id) return;

    try {
        if (action === "approve") {
            await apiRequest(`/api/admin/recipes/${id}/approve`, { method: "POST" });
        } else if (action === "disable") {
            await apiRequest(`/api/admin/recipes/${id}/disable`, { method: "POST" });
        }
        await loadPendingRecipes();
    } catch (err) {
        alert("Admin action failed: " + (err.message || ""));
    }
}

// ---------------- INIT ----------------
window.addEventListener("DOMContentLoaded", () => {
    initAuthTabs();
    updateAuthUI();

    $("loginForm").addEventListener("submit", handleLogin);
    $("registerForm").addEventListener("submit", handleRegister);
    $("logoutBtn").addEventListener("click", handleLogout);

    $("refreshRecipesBtn").addEventListener("click", () => loadRecipes());
    $("searchBtn").addEventListener("click", () => {
        const q = $("searchInput").value;
        loadRecipes(q);
    });

    $("createRecipeForm").addEventListener("submit", handleCreateRecipe);

    $("loadPendingBtn").addEventListener("click", loadPendingRecipes);
    $("pendingRecipesList").addEventListener("click", handlePendingClick);

    // Load initial recipes (public, no auth)
    loadRecipes();
});
