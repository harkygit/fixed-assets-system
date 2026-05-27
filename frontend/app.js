const integrationBaseUrl = "http://localhost:8081";

const table = document.getElementById("objectsTable");
const statusDiv = document.getElementById("status");

loadObjects();

async function loadObjects() {
    statusDiv.innerHTML = "Загрузка объектов основных средств...";

    try {
        const response = await fetch(`${integrationBaseUrl}/api/assets`);
        const objects = await response.json();

        table.innerHTML = "";

        objects.forEach(object => {
            const row = document.createElement("tr");

            row.innerHTML = `
                <td>${object.inventoryId}</td>
                <td>${object.name}</td>
                <td>${object.cost}</td>
                <td>
                    <button
                        class="btn btn-primary"
                        onclick="disposeObject('${object.inventoryId}')">
                        Списать
                    </button>
                </td>
            `;

            table.appendChild(row);
        });

        statusDiv.innerHTML = "Каталог загружен через IntegrationService";
    } catch (error) {
        statusDiv.innerHTML = "Ошибка загрузки объектов";
        console.error(error);
    }
}

async function disposeObject(inventoryId) {
    statusDiv.innerHTML = "Выполняется интеграционный сценарий...";

    try {
        const response = await fetch(
            `${integrationBaseUrl}/api/integration/fixed-assets/dispose`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    inventoryId: inventoryId,
                    usefulLife: 5,
                    disposalReason: "Физический износ"
                })
            }
        );

        const data = await response.json();

        statusDiv.innerHTML =
            `Объект ${inventoryId} списан. Годовая амортизация: ${data.depreciation.yearlyDepreciation}`;

        console.log(data);
    } catch (error) {
        statusDiv.innerHTML = "Ошибка интеграционного сценария";
        console.error(error);
    }
}
