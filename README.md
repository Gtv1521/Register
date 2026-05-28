# FrameworkDriver-Api 🚀

API REST desarrollada con ASP.NET Core para gestión de registros, observaciones, clientes y sesiones con autenticación JWT, integración con Cloudinary, WhatsApp y generación de códigos QR.

## 📋 Requisitos Previos

- **.NET 10.0** o superior
- **MongoDB** (instalado y ejecutándose localmente o en la nube)
- **Git** para control de versiones
- **Visual Studio Code** o **Visual Studio 2022+** (recomendado)

## 🛠️ Instalación

### 1. Clonar el Repositorio
```bash
git clone <repository-url>
cd Register
```

### 2. Restaurar Dependencias
```bash
dotnet restore
```

### 3. Compilar el Proyecto
```bash
dotnet build
```

## ⚙️ Configuración

### 1. Base de Datos (MongoDB)

Editar el archivo `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "mongodb://localhost:27017",
    "DatabaseName": "DataPqr"
  }
}
```

Para desarrollo local, editar `appsettings.Development.json`.

### 2. Variables de Entorno Requeridas

Crear un archivo `appsettings.Development.json` con las siguientes configuraciones:

```json
{
  "Jwt": {
    "Key": "your-super-secret-key-min-256-characters",
    "Issuer": "your-app-issuer",
    "Audience": "your-app-audience"
  },
  "Cloudinary": {
    "ApiKey": "your-cloudinary-api-key",
    "ApiSecret": "your-cloudinary-api-secret",
    "CloudName": "your-cloud-name"
  },
  "Whatsapp": {
    "ApiKey": "your-whatsapp-api-key",
    "PhoneId": "your-phone-id"
  },
  "Email": {
    "ApiKey": "your-email-service-api-key",
    "SenderEmail": "sender@example.com",
    "SenderName": "Your App Name"
  }
}
```

> ⚠️ **IMPORTANTE**: Nunca commitear `appsettings.Development.json` al repositorio. Agregar a `.gitignore`.

## 🚀 Ejecutar el Proyecto

### Modo Desarrollo (con hot reload)
```bash
dotnet watch run
```

### Modo Producción
```bash
dotnet run --configuration Release
```

## 📚 Documentación de la API

La documentación completa de la API está disponible en:

### **Swagger UI** (Interfaz Interactiva)
```
http://localhost:5000/swagger/index.html
```

### **Scalar** (Documentación Alternativa)
```
http://localhost:5000/scalar/v1
```

### **Archivo HTTP** (para testing)
Ver [FrameworkDriver-Api.http](./FrameworkDriver-Api.http) para ejemplos de peticiones.

## 📁 Estructura del Proyecto

```
src/
├── Controllers/        # Endpoints de la API
│   ├── ClientController.cs
│   ├── ObservationController.cs
│   ├── QrController.cs
│   ├── RegisterController.cs
│   ├── SessionController.cs
│   └── UserController.cs
│
├── Models/            # Modelos de base de datos
│   ├── ClientModel.cs
│   ├── ObservationModel.cs
│   ├── RegisterModel.cs
│   ├── SessionModel.cs
│   ├── UserModel.cs
│   └── DataContext.cs
│
├── Dto/               # Data Transfer Objects
│   ├── ClientDTO.cs
│   ├── LoginRequest.cs
│   ├── RegisterDTO.cs
│   ├── UserDto.cs
│   └── ...
│
├── Services/          # Lógica de negocio
│   ├── ClientService.cs
│   ├── ObservationService.cs
│   ├── RegisterService.cs
│   ├── UserService.cs
│   ├── EmailService.cs
│   ├── QrService.cs
│   └── SessionService.cs
│
├── Repositories/      # Acceso a datos (Database Layer)
│   ├── ClientRepository.cs
│   ├── ObservationRepository.cs
│   ├── RegisterRepository.cs
│   ├── SessionRepository.cs
│   └── UserRepository.cs
│
├── Interfaces/        # Contratos de servicios
│   ├── ICrud.cs
│   ├── IEmail.cs
│   ├── IHashPass.cs
│   ├── ISession.cs
│   ├── IToken.cs
│   └── ...
│
├── Utils/             # Utilidades y helpers
│   ├── Token.cs       # Generación de JWT
│   ├── Argon2Hasher.cs
│   ├── FileUpload.cs  # Manejo de archivos con Cloudinary
│   ├── WhatsappUtility.cs
│   └── Context.cs
│
└── Projections/       # Proyecciones de datos
    ├── ListRegisters.cs
    └── RegisterProjection.cs
```

## 🔐 Autenticación

El proyecto utiliza **JWT (JSON Web Tokens)** para autenticación:

1. El usuario se autentica enviando credenciales al endpoint de login
2. Recibe un token JWT que lo guarda en cookies o en el header `Authorization: Bearer <token>`
3. Los endpoints protegidos validan el token en cada solicitud

### Endpoints de Autenticación
- `POST /api/user/login` - Iniciar sesión
- `POST /api/user/register` - Registrar nuevo usuario
- `POST /api/user/refresh` - Refrescar token

## 🌐 Integraciones Externas

### 1. **MongoDB**
- Base de datos NoSQL
- Configuración: `appsettings.json` > `ConnectionStrings`

### 2. **Cloudinary**
- Almacenamiento de archivos en la nube
- Configuración: `appsettings.Development.json` > `Cloudinary`
- Uso: [Utils/FileUpload.cs](./src/Utils/FileUpload.cs)

### 3. **WhatsApp API**
- Envío de mensajes de WhatsApp
- Configuración: `appsettings.Development.json` > `Whatsapp`
- Uso: [Utils/WhatsappUtility.cs](./src/Utils/WhatsappUtility.cs)

### 4. **Servicio de Email**
- Envío de correos electrónicos
- Configuración: `appsettings.Development.json` > `Email`
- Uso: [Services/EmailService.cs](./src/Services/EmailService.cs)

## 📊 Generación de Códigos QR

El proyecto incluye funcionalidad para generar códigos QR:
- Archivo: [Services/QrService.cs](./src/Services/QrService.cs)
- Endpoint: `GET /api/qr/generate`

## 📝 Lineamientos de Desarrollo

### Arquitectura
- **Arquitectura de Capas**: Controllers → Services → Repositories → Models
- **Patrón Repository**: Abstracción del acceso a datos
- **Inyección de Dependencias**: Configurada en `Program.cs`

### Convenciones de Código

#### Nombramiento
- **Clases**: PascalCase (ej: `UserService`, `ClientController`)
- **Métodos**: PascalCase (ej: `GetUserById`, `SaveRegister`)
- **Variables locales**: camelCase (ej: `userId`, `userName`)
- **Constantes**: UPPER_CASE (ej: `MAX_RETRIES`, `API_KEY`)

#### Estructura de Clases
```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;
    private readonly IRepository<MyModel> _repository;

    public MyService(ILogger<MyService> logger, IRepository<MyModel> repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<MyModel> GetByIdAsync(string id)
    {
        // Implementación
    }
}
```

### DTOs (Data Transfer Objects)
- Usar DTOs para las respuestas de API
- Los DTOs deben ser simples y no contener lógica
- Ejemplo: [Dto/UserDto.cs](./src/Dto/UserDto.cs)

### Manejo de Errores
- Usar excepciones personalizadas en [Exceptions/](./src/Exceptions/)
- Retornar mensajes de error claros en las respuestas
- Loguear errores importantes

### Autenticación y Autorización
- Proteger endpoints con atributo `[Authorize]`
- Validar roles de usuario cuando sea necesario
- No exponer información sensible en logs

### Async/Await
- Usar `async/await` para operaciones de I/O
- Métodos async deben terminar en `Async`
- Ejemplo: `GetUserByIdAsync()`

### Validación
- Validar entrada de datos en los DTOs o Models
- Usar validaciones de flujo en Services
- Retornar errores descriptivos

## 🧪 Testing

Para ejecutar pruebas (si existen):
```bash
dotnet test
```

## 📦 Dependencias Principales

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| MongoDB.Driver | 3.5.2 | Base de datos NoSQL |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.1 | Autenticación JWT |
| CloudinaryDotNet | 1.27.9 | Integración Cloudinary |
| Isopoh.Cryptography.Argon2 | 2.0.0 | Hash de contraseñas |
| QRCoder | 1.7.0 | Generación de QR |
| RestSharp | 113.0.0 | Cliente HTTP |
| Scalar.AspNetCore | 2.11.10 | Documentación API |

## 🐛 Troubleshooting

### Problema: "No se puede conectar a MongoDB"
**Solución**: Verificar que MongoDB está ejecutándose y que la cadena de conexión es correcta en `appsettings.json`.

### Problema: "Token JWT no válido"
**Solución**: Verificar que la clave JWT en `appsettings.Development.json` coincide en la configuración de validación.

### Problema: "Error de CORS"
**Solución**: Configurar CORS correctamente en `Program.cs` si la API es llamada desde otro dominio.

## 🤝 Contribuir

1. Crear una rama para tu feature: `git checkout -b feature/mi-feature`
2. Hacer commit de los cambios: `git commit -am 'Agregar nueva feature'`
3. Push a la rama: `git push origin feature/mi-feature`
4. Abrir un Pull Request

### Reglas de Commits
- Usar mensajes descriptivos
- Ejemplo: `feat: agregar endpoint de login`, `fix: corregir validación de email`

## 📞 Contacto y Soporte

Para reportar bugs o solicitar features, contactar al equipo de desarrollo.

## 📄 Licencia

Especificar la licencia del proyecto aquí.

---

**Última actualización**: Febrero 2026
**Versión de .NET**: 10.0
**Base de Datos**: MongoDB
