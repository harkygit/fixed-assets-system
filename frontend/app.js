const integrationBaseUrl = "http://localhost:8081";

const state = {
    assets: [],
    filteredAssets: [],
    archivedIds: new Set(),
    processingIds: new Set(),
    errorCount: 0,
    selectedAssetId: null,
    cacheWarm: false,
    sort: "name-asc",
    filter: "all",
    query: ""
};

const fallbackAssets = [
    {
        inventoryId: "OS001",
        name: "Ноутбук Lenovo ThinkPad T14",
        cost: 80000
    },
    {
        inventoryId: "OS002",
        name: "МФУ HP LaserJet Pro",
        cost: 25000
    },
    {
        inventoryId: "OS003",
        name: "Сервер Dell PowerEdge R350",
        cost: 410000
    }
];

const elements = {
    table: document.getElementById("objectsTable"),
    loading: document.getElementById("loadingState"),
    empty: document.getElementById("emptyState"),
    assetSearch: document.getElementById("assetSearch"),
    globalSearch: document.getElementById("globalSearch"),
    statusFilter: document.getElementById("statusFilter"),
    sortSelect: document.getElementById("sortSelect"),
    totalAssets: document.getElementById("totalAssets"),
    activeAssets: document.getElementById("activeAssets"),
    archivedAssets: document.getElementById("archivedAssets"),
    depreciationTotal: document.getElementById("depreciationTotal"),
    errorCount: document.getElementById("errorCount"),
    integrationStatus: document.getElementById("integrationStatus"),
    redisStatus: document.getElementById("redisStatus"),
    postgresStatus: document.getElementById("postgresStatus"),
    apiStatus: document.getElementById("apiStatus"),
    redisHint: document.getElementById("redisHint"),
    activityFeed: document.getElementById("activityFeed"),
    notificationList: document.getElementById("notificationList"),
    refreshButton: document.getElementById("refreshButton"),
    healthRefreshButton: document.getElementById("healthRefreshButton"),
    runDemoButton: document.getElementById("runDemoButton"),
    themeToggle: document.getElementById("themeToggle"),
    mobileMenu: document.querySelector(".mobile-menu"),
    sidebar: document.querySelector(".sidebar"),
    assetModalTitle: document.getElementById("assetModalTitle"),
    assetModalBody: document.getElementById("assetModalBody"),
    workflowText: document.getElementById("workflowText"),
    disposalReason: document.getElementById("disposalReason"),
    confirmDisposalButton: document.getElementById("confirmDisposalButton"),
    toast: document.getElementById("appToast"),
    toastIcon: document.getElementById("toastIcon"),
    toastTitle: document.getElementById("toastTitle"),
    toastMessage: document.getElementById("toastMessage")
};

const assetModal = new bootstrap.Modal(document.getElementById("assetModal"));
const workflowModal = new bootstrap.Modal(document.getElementById("workflowModal"));
const toast = new bootstrap.Toast(elements.toast, { delay: 3200 });

document.addEventListener("DOMContentLoaded", initialize);

function initialize() {
    bindEvents();
    hydrateTheme();
    seedTimeline();
    refreshHealth();
    loadObjects();
    startRealtimeSimulation();
}

function bindEvents() {
    elements.assetSearch.addEventListener("input", event => updateQuery(event.target.value));
    elements.globalSearch.addEventListener("input", event => updateQuery(event.target.value));

    elements.statusFilter.addEventListener("change", event => {
        state.filter = event.target.value;
        applyFilters();
    });

    elements.sortSelect.addEventListener("change", event => {
        state.sort = event.target.value;
        applyFilters();
    });

    elements.refreshButton.addEventListener("click", loadObjects);
    elements.healthRefreshButton.addEventListener("click", refreshHealth);
    elements.runDemoButton.addEventListener("click", () => {
        const firstActiveAsset = state.assets.find(asset => getAssetStatus(asset) === "Active");
        if (firstActiveAsset) {
            openWorkflow(firstActiveAsset.inventoryId);
        }
    });

    elements.confirmDisposalButton.addEventListener("click", confirmDisposal);
    elements.themeToggle.addEventListener("click", toggleTheme);
    elements.mobileMenu.addEventListener("click", () => elements.sidebar.classList.toggle("open"));
}

async function loadObjects() {
    setLoading(true);
    setBadge(elements.apiStatus, "processing", "Syncing");

    try {
        const response = await fetch(`${integrationBaseUrl}/api/assets`);

        if (!response.ok) {
            throw new Error(`API returned ${response.status}`);
        }

        const assets = await response.json();
        state.assets = enrichAssets(assets);
        state.cacheWarm = true;

        setBadge(elements.apiStatus, "active", "Online");
        setBadge(elements.redisStatus, "active", "Warm");
        elements.redisHint.textContent = "cache hit ready";
        pushNotification("Каталог обновлен", "Данные получены через IntegrationService", "bi-cloud-check");
        addActivity("Каталог ОС синхронизирован", "GET /api/assets");
    } catch (error) {
        state.errorCount += 1;
        state.assets = enrichAssets(fallbackAssets);

        setBadge(elements.apiStatus, "error", "Fallback");
        setBadge(elements.redisStatus, "processing", "Local");
        pushNotification("API недоступен", "Показаны demo-данные, проверьте IntegrationService", "bi-exclamation-triangle");
        addActivity("Ошибка загрузки API", error.message);
    } finally {
        applyFilters();
        setLoading(false);
    }
}

function enrichAssets(assets) {
    return assets.map((asset, index) => ({
        ...asset,
        owner: index % 2 === 0 ? "Finance Ops" : "IT Infrastructure",
        location: index % 2 === 0 ? "Москва, HQ" : "Склад IT",
        usefulLife: index % 2 === 0 ? 5 : 4,
        integration: index % 3 === 0 ? "Synced" : "Verified"
    }));
}

function applyFilters() {
    const query = state.query.trim().toLowerCase();

    state.filteredAssets = state.assets
        .filter(asset => {
            const matchesQuery =
                asset.inventoryId.toLowerCase().includes(query) ||
                asset.name.toLowerCase().includes(query);
            const status = getAssetStatus(asset);
            const matchesStatus = state.filter === "all" || status === state.filter;

            return matchesQuery && matchesStatus;
        })
        .sort(sortAssets);

    renderTable();
    renderMetrics();
}

function sortAssets(left, right) {
    switch (state.sort) {
        case "cost-desc":
            return right.cost - left.cost;
        case "cost-asc":
            return left.cost - right.cost;
        case "id-asc":
            return left.inventoryId.localeCompare(right.inventoryId);
        default:
            return left.name.localeCompare(right.name);
    }
}

function renderTable() {
    elements.table.innerHTML = "";
    elements.empty.classList.toggle("d-none", state.filteredAssets.length > 0);

    state.filteredAssets.forEach(asset => {
        const row = document.createElement("tr");
        const status = getAssetStatus(asset);
        const statusClass = status.toLowerCase();

        row.innerHTML = `
            <td>
                <div class="asset-name-cell">
                    <div class="asset-avatar"><i class="bi bi-hdd-stack"></i></div>
                    <div>
                        <div class="asset-title">${asset.name}</div>
                        <div class="asset-id">${asset.inventoryId} · ${asset.owner}</div>
                    </div>
                </div>
            </td>
            <td>${formatCurrency(asset.cost)}</td>
            <td>${formatCurrency(calculateDepreciation(asset))}</td>
            <td><span class="status-badge ${statusClass}">${status}</span></td>
            <td>
                <span class="status-badge active">
                    <i class="bi bi-check2 me-1"></i>${asset.integration}
                </span>
            </td>
            <td class="text-end">
                <div class="dropdown">
                    <button class="btn action-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                        <i class="bi bi-three-dots"></i>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li>
                            <button class="dropdown-item" type="button" data-action="details" data-id="${asset.inventoryId}">
                                <i class="bi bi-card-text me-2"></i>Детали
                            </button>
                        </li>
                        <li>
                            <button class="dropdown-item" type="button" data-action="depreciation" data-id="${asset.inventoryId}">
                                <i class="bi bi-graph-up-arrow me-2"></i>Амортизация
                            </button>
                        </li>
                        <li>
                            <button class="dropdown-item" type="button" data-action="dispose" data-id="${asset.inventoryId}">
                                <i class="bi bi-archive me-2"></i>Списать
                            </button>
                        </li>
                    </ul>
                </div>
            </td>
        `;

        elements.table.appendChild(row);
    });

    elements.table.querySelectorAll("[data-action]").forEach(button => {
        button.addEventListener("click", () => handleAction(button.dataset.action, button.dataset.id));
    });
}

function renderMetrics() {
    const activeCount = state.assets.filter(asset => getAssetStatus(asset) === "Active").length;
    const archivedCount = state.assets.filter(asset => getAssetStatus(asset) === "Archived").length;
    const depreciation = state.assets.reduce((sum, asset) => sum + calculateDepreciation(asset), 0);

    elements.totalAssets.textContent = state.assets.length;
    elements.activeAssets.textContent = activeCount;
    elements.archivedAssets.textContent = archivedCount;
    elements.depreciationTotal.textContent = formatCurrency(depreciation);
    elements.errorCount.textContent = state.errorCount;
}

function handleAction(action, assetId) {
    if (action === "details") {
        openDetails(assetId);
    }

    if (action === "depreciation") {
        openDepreciation(assetId);
    }

    if (action === "dispose") {
        openWorkflow(assetId);
    }
}

function openDetails(assetId) {
    const asset = findAsset(assetId);
    if (!asset) return;

    elements.assetModalTitle.textContent = asset.name;
    elements.assetModalBody.innerHTML = `
        <div class="details-grid">
            <div class="detail-tile"><span>Inventory ID</span><strong>${asset.inventoryId}</strong></div>
            <div class="detail-tile"><span>Статус</span><strong>${getAssetStatus(asset)}</strong></div>
            <div class="detail-tile"><span>Стоимость</span><strong>${formatCurrency(asset.cost)}</strong></div>
            <div class="detail-tile"><span>Годовая амортизация</span><strong>${formatCurrency(calculateDepreciation(asset))}</strong></div>
            <div class="detail-tile"><span>Владелец</span><strong>${asset.owner}</strong></div>
            <div class="detail-tile"><span>Локация</span><strong>${asset.location}</strong></div>
        </div>
    `;

    assetModal.show();
}

function openDepreciation(assetId) {
    const asset = findAsset(assetId);
    if (!asset) return;

    elements.assetModalTitle.textContent = `Амортизация: ${asset.inventoryId}`;
    elements.assetModalBody.innerHTML = `
        <div class="details-grid">
            <div class="detail-tile"><span>Первоначальная стоимость</span><strong>${formatCurrency(asset.cost)}</strong></div>
            <div class="detail-tile"><span>Срок полезного использования</span><strong>${asset.usefulLife} лет</strong></div>
            <div class="detail-tile"><span>Метод</span><strong>Линейный</strong></div>
            <div class="detail-tile"><span>Годовая амортизация</span><strong>${formatCurrency(calculateDepreciation(asset))}</strong></div>
        </div>
    `;

    assetModal.show();
}

function openWorkflow(assetId) {
    const asset = findAsset(assetId);
    if (!asset) return;

    state.selectedAssetId = assetId;
    elements.workflowText.textContent =
        `${asset.name} (${asset.inventoryId}) будет проведен через IntegrationService, DepreciationService и DisposalService.`;
    elements.disposalReason.value = "Физический износ";
    workflowModal.show();
}

async function confirmDisposal() {
    const asset = findAsset(state.selectedAssetId);
    if (!asset) return;

    state.processingIds.add(asset.inventoryId);
    applyFilters();
    workflowModal.hide();
    showToast("Processing", `Запущено списание ${asset.inventoryId}`, "processing");
    addActivity("Запуск сценария списания", asset.inventoryId);

    try {
        const response = await fetch(`${integrationBaseUrl}/api/integration/fixed-assets/dispose`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                inventoryId: asset.inventoryId,
                usefulLife: asset.usefulLife,
                disposalReason: elements.disposalReason.value
            })
        });

        if (!response.ok) {
            throw new Error(`Workflow returned ${response.status}`);
        }

        const result = await response.json();

        state.processingIds.delete(asset.inventoryId);
        state.archivedIds.add(asset.inventoryId);
        setBadge(elements.redisStatus, "active", "Warm");
        showToast("Списание завершено", result.disposal.message, "active");
        pushNotification("Workflow completed", `${asset.inventoryId} списан, correlation id создан`, "bi-check2-circle");
        addActivity("Сценарий успешно завершен", result.correlationId);
    } catch (error) {
        state.processingIds.delete(asset.inventoryId);
        state.errorCount += 1;
        showToast("Ошибка интеграции", error.message, "error");
        pushNotification("Workflow failed", error.message, "bi-exclamation-triangle");
        addActivity("Ошибка сценария списания", asset.inventoryId);
    } finally {
        applyFilters();
    }
}

async function refreshHealth() {
    setBadge(elements.integrationStatus, "processing", "Checking");

    try {
        const response = await fetch(`${integrationBaseUrl}/health`);

        if (!response.ok) {
            throw new Error("Health endpoint unavailable");
        }

        await response.json();
        setBadge(elements.integrationStatus, "active", "Healthy");
        setBadge(elements.postgresStatus, "active", "Ready");
        setBadge(elements.apiStatus, "active", "Online");
        addActivity("Health monitor обновлен", "IntegrationService healthy");
    } catch (error) {
        state.errorCount += 1;
        setBadge(elements.integrationStatus, "error", "Offline");
        setBadge(elements.apiStatus, "error", "Degraded");
        addActivity("Health check failed", error.message);
        renderMetrics();
    }
}

function startRealtimeSimulation() {
    window.setInterval(() => {
        const messages = [
            ["Redis cache", state.cacheWarm ? "assets:list cache hit" : "warming up"],
            ["API latency", `${Math.floor(Math.random() * 24) + 18} ms average`],
            ["PostgreSQL", "connection pool ready"],
            ["Polly", "retry policy armed"]
        ];

        const [title, message] = messages[Math.floor(Math.random() * messages.length)];
        addActivity(title, message);
    }, 7000);
}

function seedTimeline() {
    pushNotification("Docker Compose", "Demo stand готов к проверке", "bi-box-seam");
    pushNotification("Swagger", "OpenAPI доступен на :8081/swagger", "bi-braces");
    addActivity("Frontend initialized", "Control Center loaded");
}

function pushNotification(title, message, icon) {
    const item = document.createElement("div");
    item.className = "notification-item";
    item.innerHTML = `
        <div class="notification-icon"><i class="bi ${icon}"></i></div>
        <div>
            <strong>${title}</strong>
            <span>${message}</span>
        </div>
    `;

    elements.notificationList.prepend(item);
    trimChildren(elements.notificationList, 4);
}

function addActivity(title, message) {
    const item = document.createElement("div");
    item.className = "activity-item";
    item.innerHTML = `
        <div class="activity-icon"><i class="bi bi-activity"></i></div>
        <div>
            <strong>${title}</strong>
            <span>${message}</span>
        </div>
    `;

    elements.activityFeed.prepend(item);
    trimChildren(elements.activityFeed, 5);
}

function updateQuery(value) {
    state.query = value;
    elements.assetSearch.value = value;
    elements.globalSearch.value = value;
    applyFilters();
}

function getAssetStatus(asset) {
    if (state.processingIds.has(asset.inventoryId)) return "Processing";
    if (state.archivedIds.has(asset.inventoryId)) return "Archived";
    return "Active";
}

function calculateDepreciation(asset) {
    return asset.cost / asset.usefulLife;
}

function findAsset(assetId) {
    return state.assets.find(asset => asset.inventoryId === assetId);
}

function formatCurrency(value) {
    return new Intl.NumberFormat("ru-RU", {
        style: "currency",
        currency: "RUB",
        maximumFractionDigits: 0
    }).format(value);
}

function setLoading(isLoading) {
    elements.loading.classList.toggle("d-none", !isLoading);
    document.querySelector(".table-shell").classList.toggle("d-none", isLoading);
}

function setBadge(element, type, label) {
    element.className = `status-badge ${type}`;
    element.textContent = label;
}

function showToast(title, message, type) {
    const iconMap = {
        active: "bi-check-circle",
        processing: "bi-arrow-repeat",
        error: "bi-exclamation-triangle"
    };

    elements.toastIcon.className = `bi ${iconMap[type] || "bi-info-circle"}`;
    elements.toastTitle.textContent = title;
    elements.toastMessage.textContent = message;
    toast.show();
}

function toggleTheme() {
    const nextTheme = document.documentElement.dataset.theme === "dark" ? "light" : "dark";
    document.documentElement.dataset.theme = nextTheme;
    localStorage.setItem("fixedAssetsTheme", nextTheme);
    elements.themeToggle.innerHTML = nextTheme === "dark"
        ? '<i class="bi bi-sun"></i>'
        : '<i class="bi bi-moon-stars"></i>';
}

function hydrateTheme() {
    const savedTheme = localStorage.getItem("fixedAssetsTheme") || "light";
    document.documentElement.dataset.theme = savedTheme;
    elements.themeToggle.innerHTML = savedTheme === "dark"
        ? '<i class="bi bi-sun"></i>'
        : '<i class="bi bi-moon-stars"></i>';
}

function trimChildren(container, maxItems) {
    while (container.children.length > maxItems) {
        container.removeChild(container.lastElementChild);
    }
}
