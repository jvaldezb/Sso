# Autenticación LDAP - Guía de Configuración

## Descripción
Este documento describe cómo configurar y usar la autenticación LDAP en el servicio de identidad SSO.

## Características Implementadas

- ✅ Autenticación de usuarios contra servidor LDAP
- ✅ Creación automática de usuarios en la base de datos local al primer login
- ✅ Sincronización de información del usuario (nombre, email, etc.)
- ✅ Integración con el flujo de tokens existente (JWT + Refresh Token)
- ✅ Soporte para múltiples sistemas a través de roles

## Configuración

### 1. Configurar el servidor LDAP

Edita el archivo `appsettings.Development.json` o `appsettings.json`:

```json
{
  "LdapSettings": {
    "Server": "ldap.example.com",        // Dirección del servidor LDAP
    "Port": "389",                        // Puerto LDAP (389 para no-SSL, 636 para SSL)
    "BaseDn": "dc=example,dc=com",       // Base DN de tu organización
    "UserSearchBase": "ou=users",         // OU donde se encuentran los usuarios
    "UseSsl": "false",                    // true para conexión segura (LDAPS)
    "Enabled": "true"                     // true para habilitar LDAP
  }
}
```

### 2. Estructura LDAP Esperada

El servicio asume la siguiente estructura:

```
dc=example,dc=com
  └── ou=users
      ├── uid=usuario1
      ├── uid=usuario2
      └── ...
```

Los usuarios deben tener los siguientes atributos (opcionales):
- `uid`: Identificador único del usuario
- `cn`: Common Name
- `mail`: Email del usuario
- `displayName`: Nombre para mostrar
- `givenName`: Nombre
- `sn`: Apellido
- `memberOf`: Grupos a los que pertenece

## Uso

### Endpoint de Login LDAP

```http
POST /api/auth/login-ldap
Content-Type: application/json

{
  "username": "usuario.ldap",
  "password": "password123"
}
```

### Respuesta Exitosa

```json
{
  "isSuccess": true,
  "data": {
    "userId": "guid-del-usuario",
    "fullName": "Nombre Usuario",
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "accessTokenExpires": "2025-12-24T15:30:00Z",
    "refreshToken": "refresh-token-aqui",
    "refreshTokenExpires": "2025-12-31T15:30:00Z",
    "ssoSystemId": 1,
    "systems": [...],
    "menus": [...],
    "roles": [...]
  }
}
```

### Respuesta de Error

```json
{
  "isSuccess": false,
  "errorMessage": "Invalid LDAP credentials"
}
```

## Flujo de Autenticación

1. **Cliente envía credenciales LDAP** → POST `/api/auth/login-ldap`
2. **Servicio autentica contra LDAP** → Valida usuario y contraseña
3. **Recupera información del usuario** → Obtiene atributos LDAP
4. **Busca o crea usuario local**:
   - Si el usuario no existe → Lo crea en la base de datos local
   - Si existe → Actualiza su información
5. **Genera tokens JWT**:
   - AccessToken (60 minutos)
   - RefreshToken
6. **Registra sesión** → Guarda información de la sesión
7. **Retorna tokens y datos** → Cliente recibe los tokens y sistemas disponibles

## Mapeo de Usuarios

Cuando un usuario se autentica por primera vez vía LDAP, se crea en la base de datos local con:

| Campo BD | Valor LDAP/Generado |
|----------|---------------------|
| `UserName` | `uid` del LDAP |
| `Email` | `mail` del LDAP |
| `FullName` | `displayName` o `cn` del LDAP |
| `DocumentType` | "LDAP" (fijo) |
| `DocumentNumber` | `uid` del LDAP |
| `IsEnabled` | `true` |
| `EmailConfirmed` | `true` |
| `UserCreate` | "LDAP_SYSTEM" |

## Gestión de Roles y Permisos

Los usuarios LDAP necesitan roles asignados manualmente en el sistema:

1. El usuario se autentica por primera vez
2. Un administrador asigna roles al usuario
3. Los roles determinan a qué sistemas tiene acceso
4. En futuros logins, el usuario obtendrá los tokens con los permisos correspondientes

## Consideraciones de Seguridad

### Producción
- ✅ Usa SSL/TLS para conexiones LDAP (`UseSsl: true`)
- ✅ Valida certificados del servidor LDAP correctamente
- ✅ No almacena contraseñas LDAP en la base de datos local
- ✅ Usa variables de entorno para configuración sensible

### Desarrollo
- ⚠️ Puedes usar LDAP sin SSL para pruebas locales
- ⚠️ Asegúrate de que el servidor LDAP de desarrollo esté aislado

## Validación de Certificados SSL

Para producción, modifica la validación de certificados en [LdapAuthenticationService.cs](../identity-service/Services/LdapAuthenticationService.cs):

```csharp
if (useSsl)
{
    connection.SessionOptions.SecureSocketLayer = true;
    connection.SessionOptions.VerifyServerCertificate = (conn, cert) => 
    {
        // Implementar validación correcta del certificado
        // Por ejemplo, usar X509Certificate2 y validar contra CA conocida
        return true; // Solo para desarrollo
    };
}
```

## Troubleshooting

### Error: "LDAP authentication is not enabled"
- Verifica que `LdapSettings:Enabled` esté en `true`

### Error: "LDAP authentication failed"
- Verifica las credenciales del usuario
- Confirma que el servidor LDAP esté accesible
- Revisa los logs del servidor para más detalles

### Error: "SSO Central system not configured"
- Debes tener un sistema con `IsCentralAdmin = true` en la tabla `SystemRegistries`

### Error: "Failed to create user"
- Verifica que el email del usuario LDAP sea único
- Revisa las reglas de validación de Identity Framework

## Testing

### Con LDAP real:
```bash
curl -X POST http://localhost:5000/api/auth/login-ldap \
  -H "Content-Type: application/json" \
  -d '{"username":"usuario.ldap","password":"password123"}'
```

### Verificar disponibilidad del servicio:
El servicio incluye un método `IsAvailableAsync()` que puede ser usado para health checks.

## Archivos Modificados/Creados

1. **Nuevos archivos**:
   - `/Services/LdapAuthenticationService.cs` - Servicio principal de LDAP
   - `/Services/Interfaces/ILdapAuthenticationService.cs` - Interfaz del servicio
   - `/Dtos/Auth/LoginLdapDto.cs` - DTO para login LDAP

2. **Archivos modificados**:
   - `/Controllers/AuthController.cs` - Agregado endpoint `login-ldap`
   - `/Services/AuthService.cs` - Agregado método `LoginLdapAsync`
   - `/Services/Interfaces/IAuthService.cs` - Agregada firma del método
   - `/Program.cs` - Registrado servicio LDAP en DI
   - `/appsettings.json` - Agregada configuración LDAP
   - `/appsettings.Development.json` - Agregada configuración LDAP
   - `/identity-service.csproj` - Agregado paquete System.DirectoryServices.Protocols

## Próximos Pasos Recomendados

1. **Sincronización de Grupos**: Mapear grupos LDAP a roles del sistema automáticamente
2. **Cache**: Implementar cache para información de usuarios LDAP
3. **Health Check**: Agregar endpoint para verificar conectividad LDAP
4. **Auditoría**: Registrar todos los intentos de login LDAP en `AuthAuditLog`
5. **Multiple LDAP**: Soporte para múltiples servidores LDAP
