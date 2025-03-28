# RaftNode

## Descripción
RaftNode es una implementación basada en ASP.NET Core del protocolo de consenso **Raft** para manejar replicación de datos y tolerancia a fallos en un clúster distribuido. Este proyecto permite crear nodos que se comunican entre sí para alcanzar consenso sobre el estado del sistema.

## Características
- Implementación del protocolo Raft para consenso distribuido.
- Comunicación HTTP entre nodos.
- Manejo de particiones de red y fallos de nodos.
- Persistencia opcional del estado.
- Logging detallado de transiciones de estado y mensajes entre nodos.

## Requisitos
- .NET 7.0 o superior
- SDK de .NET instalado en el sistema

## Instalación y Configuración
1. Clona este repositorio:
   ```sh
   git clone [https://github.com/matlab98/DistributedSystemRaft.git]
   cd RaftNode
   ```

2. Restaura las dependencias:
   ```sh
   dotnet restore
   ```
## Ejecución
Para iniciar el clúster con tres nodos en diferentes puertos, ejecuta los siguientes comandos en PowerShell:
```powershell
Start-Process "dotnet" -ArgumentList "run 3262"
Start-Process "dotnet" -ArgumentList "run 3263"
Start-Process "dotnet" -ArgumentList "run 3264"
```

Cada comando inicia un nodo en un puerto diferente (3262, 3263 y 3264). Puedes modificar estos valores según tu configuración.

## Endpoints Disponibles
- **GET /api/raft/value** → Obtiene el valor almacenado en el nodo actual.
- **GET /api/raft/leader** → Redirige al líder del clúster si el nodo no es el líder.

## Manejo de Fallos y Particiones de Red
El sistema detecta y maneja automáticamente la desconexión de nodos y cambios en la estructura del clúster:
1. Si un nodo se desconecta, los demás siguen funcionando y el clúster intenta reelegir un líder si es necesario.
2. Cuando un nodo vuelve a estar disponible, se sincroniza con el clúster y recupera su estado.

## Logging
El sistema registra:
- Transiciones de estado de los nodos (líder, seguidor, candidato).
- Mensajes de comunicación entre nodos.
- Errores y eventos críticos.

