<!-- Ruta: /README.md | V4.0 (Paso D en curso) -->

# Planta.Api

API .NET para operación de Planta (recepción, catálogos, procesos y stock).  
**Estado del proyecto**:  
- ✅ Módulo A — Integridad & Rendimiento DB (completado)  
- ✅ Módulo B — Catálogos + Cache/ETag (completado)  
- ✅ Módulo C — Recibos (App + API) (completado)  
- 🚧 Módulo D — Proceso de Trituración (en curso)  
- ⏭ Módulo E — Stock & Trazabilidad (próximo)

---

## Requisitos

- **.NET 8 SDK**
- **SQL Server** (Developer/Express/Container)
- PowerShell/Bash para ejecutar scripts
- (Opcional) Postman/Insomnia/cURL para llamadas de verificación

---

## Configuración

1) **Cadena de conexión (Dev)**  
   En `Planta.Api/appsettings.Development.json` define `ConnectionStrings.Default`.  
   Ejemplo:
   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=localhost,1433;Database=PlantaDb;User Id=sa;Password=<tuPass>;TrustServerCertificate=true"
     }
   }
