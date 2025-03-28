# DistributedSystemRaft

Instalación y Configuración

Clona este repositorio:

git clone https://github.com/matlab98/DistributedSystemRaft.git
cd RaftNode

Restaura las dependencias:

dotnet restore

Compila el proyecto:

dotnet build

Ejecución

Para iniciar el clúster con tres nodos en diferentes puertos, ejecuta los siguientes comandos en PowerShell:

Start-Process "dotnet" -ArgumentList "run 3262"
Start-Process "dotnet" -ArgumentList "run 3263"
Start-Process "dotnet" -ArgumentList "run 3264"

Cada comando inicia un nodo en un puerto diferente (3262, 3263 y 3264). Puedes modificar estos valores según tu configuración.

Endpoints Disponibles

GET /value → Obtiene el valor almacenado en el nodo actual.

GET /leader → Redirige al líder del clúster si el nodo no es el líder.

Manejo de Fallos y Particiones de Red

El sistema detecta y maneja automáticamente la desconexión de nodos y cambios en la estructura del clúster:

Si un nodo se desconecta, los demás siguen funcionando y el clúster intenta reelegir un líder si es necesario.

Cuando un nodo vuelve a estar disponible, se sincroniza con el clúster y recupera su estado.

Logging

El sistema registra:

Transiciones de estado de los nodos (líder, seguidor, candidato).

Mensajes de comunicación entre nodos.

Errores y eventos críticos.