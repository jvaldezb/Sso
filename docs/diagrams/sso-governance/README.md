# Modelo de Gobernanza, Roles y Accesos del Sistema de Autenticación Única (SSO)

El presente apartado describe el modelo de gobernanza, administración de roles y control de accesos del Sistema de Autenticación Única (SSO), así como su relación con los sistemas integrados.  
La explicación se apoya en tres diagramas conceptuales elaborados en lenguaje Mermaid (`.mmd`), los cuales permiten comprender de manera progresiva el funcionamiento del SSO desde su arranque hasta la operación diaria.

## Diagrama completo
```mermaid
flowchart LR

    %% ===============================
    %% CATEGORÍA 0 – USUARIO TÉCNICO
    %% ===============================
    subgraph CAT0[CAT 0]
        direction TB
        H0[Categoría 0 - Usuario Técnico del SSO]

        T0[Usuario Técnico del SSO]
        T1[Configuración inicial del SSO]
        T2[Designar Super Administradores del SSO]
        T3[Recuperación administrativa del SSO]
        T4[Uso restringido y excepcional]

        H0 --> T0
        T0 --> T1
        T0 --> T2
        T0 --> T3
        T0 --> T4
    end

    %% ===============================
    %% CATEGORÍA 1 – SUPER ADMIN SSO
    %% ===============================
    subgraph CAT1[CAT 1]
        direction TB
        H1[Categoría 1 - Super Administradores del SSO]

        SA[Super Admin SSO]

        A1[Administrar sistemas registrados]
        A2[Administrar usuarios globales]
        A3[Administrar roles globales]
        A4[Supervisar operación del SSO]
        A5[Designar administradores de sistemas]

        H1 --> SA
        SA --> A1
        SA --> A2
        SA --> A3
        SA --> A4
        SA --> A5
    end

    %% ===============================
    %% CATEGORÍA 2 – ADMIN SISTEMA
    %% ===============================
    subgraph CAT2[CAT 2]
        direction TB
        H2[Categoría 2 - Administrador de Sistema]

        AS[Administrador de Sistema]

        B1[Administrar usuarios de su sistema]
        B2[Asignar roles de su sistema]
        B3[Acceso parcial al portal SSO]
        B4[Operación del sistema asignado]

        H2 --> AS
        AS --> B1
        AS --> B2
        AS --> B3
        AS --> B4
    end

    %% ===============================
    %% CATEGORÍA 3 – USUARIO FINAL
    %% ===============================
    subgraph CAT3[CAT 3]
        direction TB
        H3[Categoría 3 - Usuario de Sistemas]

        U[Usuario]

        C1[Autenticación mediante SSO]
        C2[Acceso a sistemas autorizados]
        C3[Uso funcional del sistema]

        H3 --> U
        U --> C1
        U --> C2
        U --> C3
    end

    %% ===============================
    %% RELACIONES DE DESIGNACIÓN
    %% ===============================
    T2 -.->|designa| SA
    SA -.->|designa| AS
    AS -.->|gestiona| U

    %% ===============================
    %% ESTILOS DARK MODE
    %% ===============================
    classDef header fill:transparent,stroke:transparent,color:#bdc3c7,font-weight:bold;
    class H0,H1,H2,H3 header;

    classDef tecnico fill:#2c3e50,stroke:#95a5a6,stroke-width:2px,color:#ecf0f1,font-weight:bold;
    classDef super fill:#3b1f1f,stroke:#e74c3c,stroke-width:2px,color:#ecf0f1;
    classDef admin fill:#3a2f1a,stroke:#f39c12,stroke-width:2px,color:#ecf0f1;
    classDef user fill:#1f3a2e,stroke:#2ecc71,stroke-width:2px,color:#ecf0f1;

    class T0 tecnico
    class SA super
    class AS admin
    class U user
```

---

## 1. Gobernanza y Arranque del Sistema de Autenticación Única

**Referencia:** *Diagrama 1 – Gobernanza del SSO (Arranque y Administración)*
```mermaid
flowchart TB

    %% ===============================
    %% CATEGORÍA 0 – USUARIO TÉCNICO
    %% ===============================
    subgraph CAT0[CAT 0]
        direction TB
        H0[Usuario Técnico del SSO]

        T0[Configuración inicial del SSO]
        T1[Creación de Super Administradores del SSO]
        T2[Recuperación administrativa del SSO]
        T3[Uso restringido y excepcional]

        H0 --> T0
        H0 --> T1
        H0 --> T2
        H0 --> T3
    end

    %% ===============================
    %% CATEGORÍA 1 – SUPER ADMIN SSO
    %% ===============================
    subgraph CAT1[CAT 1]
        direction TB
        H1[Super Administradores del SSO]

        A1[Administrar sistemas registrados]
        A2[Administrar usuarios globales]
        A3[Administrar roles globales]
        A4[Supervisar operación del SSO]
        A5[Designar administradores de sistemas]

        H1 --> A1
        H1 --> A2
        H1 --> A3
        H1 --> A4
        H1 --> A5
    end

    %% ===============================
    %% RELACIÓN DE DESIGNACIÓN
    %% ===============================
    T1 -.->|designa| H1

    %% ===============================
    %% ESTILOS DARK MODE (CON BORDES)
    %% ===============================
    classDef header fill:#1e272e,stroke:#7f8c8d,stroke-width:1.5px,color:#ecf0f1,font-weight:bold;

    classDef tecnico fill:#2c3e50,stroke:#95a5a6,stroke-width:2px,color:#ecf0f1;
    classDef super fill:#3b1f1f,stroke:#e74c3c,stroke-width:2px,color:#ecf0f1;

    class H0,H1 header
    class CAT0 tecnico
    class CAT1 super

```

El primer diagrama describe el proceso de **arranque institucional del SSO** y los roles responsables de su administración a nivel global.

### Usuario Técnico del SSO (Categoría 0)

El Usuario Técnico del SSO es una cuenta especial, no asociada a una persona natural, cuya finalidad es:

- Realizar la configuración inicial del SSO.
- Crear y designar a los Super Administradores del SSO.
- Permitir la recuperación administrativa del sistema ante contingencias.
- Garantizar la continuidad operativa del SSO.

Este usuario tiene un **uso restringido y excepcional**, y no forma parte de la operación cotidiana del sistema.

### Super Administradores del SSO (Categoría 1)

Los Super Administradores del SSO representan a **personas naturales debidamente identificadas**, responsables de la administración integral del SSO una vez que el sistema se encuentra operativo.

Entre sus funciones principales se encuentran:

- Administrar los sistemas registrados en el SSO.
- Gestionar usuarios globales.
- Gestionar roles globales.
- Supervisar la operación general del SSO.
- Designar a los administradores de los sistemas integrados.

El diagrama enfatiza que el Usuario Técnico **designa** a los Super Administradores del SSO, sin implicar que se trate de la misma persona.

---

## 2. Administración y Uso de los Sistemas Integrados

**Referencia:** *Diagrama 2 – Administración y Uso de los Sistemas Integrados*
```mermaid
flowchart TB

    %% ===============================
    %% CATEGORÍA 1 – SUPER ADMIN SSO
    %% ===============================
    subgraph CAT1[CAT 1]
        direction TB
        H1[Super Administrador del SSO]

        S1[Registrar sistemas en el SSO]
        S2[Designar administradores de sistemas]

        H1 --> S1
        H1 --> S2
    end

    %% ===============================
    %% CATEGORÍA 2 – ADMIN SISTEMA
    %% ===============================
    subgraph CAT2[CAT 2]
        direction TB
        H2[Administrador de Sistema]

        A1[Administrar usuarios de su sistema]
        A2[Asignar roles del sistema]
        A3[Acceso parcial al portal SSO]
        A4[Operación del sistema]

        H2 --> A1
        H2 --> A2
        H2 --> A3
        H2 --> A4
    end

    %% ===============================
    %% CATEGORÍA 3 – USUARIO FINAL
    %% ===============================
    subgraph CAT3[CAT 3]
        direction TB
        H3[Usuario del Sistema]

        U1[Autenticarse mediante SSO]
        U2[Acceder a sistemas autorizados]
        U3[Uso funcional del sistema]

        H3 --> U1
        H3 --> U2
        H3 --> U3
    end

    %% ===============================
    %% RELACIONES DE GESTIÓN
    %% ===============================
    S2 -.->|designa| H2
    H2 -.->|gestiona| H3

    %% ===============================
    %% ESTILOS DARK MODE (CON BORDES)
    %% ===============================
    classDef header fill:#1e272e,stroke:#7f8c8d,stroke-width:1.5px,color:#ecf0f1,font-weight:bold;

    classDef super fill:#3b1f1f,stroke:#e74c3c,stroke-width:2px,color:#ecf0f1;
    classDef admin fill:#3a2f1a,stroke:#f39c12,stroke-width:2px,color:#ecf0f1;
    classDef user fill:#1f3a2e,stroke:#2ecc71,stroke-width:2px,color:#ecf0f1;

    class H1,H2,H3 header
    class CAT1 super
    class CAT2 admin
    class CAT3 user

```

El segundo diagrama explica cómo el SSO se relaciona con los sistemas integrados (como PAT, SISR u otros), una vez que la plataforma ya se encuentra en funcionamiento.

### Administrador de Sistema (Categoría 2)

El Administrador de Sistema es un usuario responsable de la gestión de un sistema específico integrado al SSO.  
Sus funciones incluyen:

- Administrar usuarios de su propio sistema.
- Asignar roles definidos por dicho sistema.
- Acceder al portal del SSO con permisos parciales.
- Supervisar la operación funcional del sistema bajo su responsabilidad.

Este rol es **designado por un Super Administrador del SSO**, lo cual representa una relación de delegación administrativa, no de jerarquía personal.

### Usuario del Sistema (Categoría 3)

El Usuario del Sistema es el usuario final que consume los servicios de uno o más sistemas integrados.

Sus capacidades se limitan a:

- Autenticarse mediante el SSO.
- Acceder únicamente a los sistemas para los cuales ha sido autorizado.
- Utilizar las funcionalidades propias del sistema según los roles asignados.

El diagrama deja explícito que el Administrador de Sistema **gestiona** usuarios, pero no implica que una misma persona deba cumplir ambos roles.

---

## 3. Separación entre Roles Globales y Roles por Sistema

**Referencia:** *Diagrama 3 – Separación de Roles Globales y Roles por Sistema*
```mermaid
flowchart TB

    %% ===============================
    %% IDENTIDAD GLOBAL
    %% ===============================
    subgraph ID[Identidad en el SSO]
        direction TB
        U[Usuario Global]
        R0[Rol Global SSO]

        U --> R0
    end

    %% ===============================
    %% ROLES GLOBALES
    %% ===============================
    subgraph RG[Roles Globales del SSO]
        direction TB
        G1[SSO_SUPER_ADMIN]
        G2[SSO_ADMIN]
        G3[SSO_AUDITOR]

        R0 --> G1
        R0 --> G2
        R0 --> G3
    end

    %% ===============================
    %% SISTEMAS INTEGRADOS
    %% ===============================
    subgraph SYS[Sistemas Integrados]
        direction TB
        S1[PAT]
        S2[SISR]
        S3[Otros Sistemas]
    end

    %% ===============================
    %% ROLES POR SISTEMA
    %% ===============================
    subgraph RS[Roles por Sistema]
        direction TB
        P1[PAT_ADMIN]
        P2[PAT_USER]

        S1 --> P1
        S1 --> P2
    end

    %% ===============================
    %% ASIGNACIÓN DE ROLES
    %% ===============================
    U -.->|asignado a| S1
    U -.->|asignado a| S2

    U -.->|tiene| G1
    U -.->|tiene| P1

    %% ===============================
    %% ESTILOS DARK MODE
    %% ===============================
    classDef identity fill:#2c3e50,stroke:#95a5a6,stroke-width:2px,color:#ecf0f1,font-weight:bold;
    classDef global fill:#3b1f1f,stroke:#e74c3c,stroke-width:2px,color:#ecf0f1;
    classDef system fill:#3a2f1a,stroke:#f39c12,stroke-width:2px,color:#ecf0f1;
    classDef roleSystem fill:#1f3a2e,stroke:#2ecc71,stroke-width:2px,color:#ecf0f1;

    class U identity
    class R0 identity
    class G1,G2,G3 global
    class S1,S2,S3 system
    class P1,P2 roleSystem
```

El tercer diagrama clarifica uno de los principios fundamentales del modelo: la **separación de responsabilidades entre el SSO y los sistemas integrados**.

### Usuario Global

El Usuario Global representa la identidad única registrada en el SSO.  
Esta identidad:

- Existe una sola vez en la plataforma.
- Puede ser asignada a uno o más sistemas.
- Puede tener roles globales y roles por sistema de forma independiente.

### Roles Globales del SSO

Los Roles Globales controlan las capacidades del usuario **dentro del SSO**, tales como:

- Administración del SSO.
- Auditoría.
- Gestión de usuarios y sistemas.

Ejemplos de roles globales:
- `SSO_SUPER_ADMIN`
- `SSO_ADMIN`
- `SSO_AUDITOR`

Estos roles **no otorgan permisos funcionales dentro de los sistemas integrados**.

### Roles por Sistema

Cada sistema integrado define y administra sus propios roles, los cuales regulan las funcionalidades internas del sistema.

Ejemplos:
- `PAT_ADMIN`
- `PAT_USER`

El SSO no interpreta ni gobierna estos roles; únicamente los transporta como parte del contexto de autenticación.

---

## 4. Principio Rector del Modelo

> El Sistema de Autenticación Única es responsable de la gestión de identidades y roles globales, mientras que los sistemas integrados son responsables de la definición y aplicación de sus roles funcionales.

Este principio garantiza:

- Separación clara de responsabilidades.
- Escalabilidad del ecosistema de sistemas.
- Facilidad de auditoría.
- Continuidad institucional ante cambios de personal.

---

## 5. Gestión de Contingencias del Sistema de Autenticación Única (SSO)

El Sistema de Autenticación Única (SSO) contempla un mecanismo formal de contingencia destinado a garantizar la continuidad operativa institucional ante situaciones excepcionales que impidan la administración normal del sistema.

---

### 5.1 Escenarios que habilitan una contingencia

- Inexistencia de SSO Super Administradores activos.
- Incidentes críticos de seguridad.
- Errores de configuración que impidan la administración del SSO.
- Eventos extraordinarios que comprometan la gobernabilidad del sistema.

---

### 5.2 Principios del Usuario Técnico del SSO

- Cuenta institucional excepcional.
- No representa a una persona natural.
- Uso temporal, justificado y auditado.
- Debe deshabilitarse tras superar la contingencia.

---

### 5.3 Flujo de activación y cierre de contingencia

```mermaid
flowchart TD
    A[Inicio: Incidente o Evento Crítico] --> B{¿Existe al menos un\nSSO Super Admin activo?}
    B -- Sí --> C[Gestionar incidente\ncon Super Admin]
    C --> Z[Fin]
    B -- No --> D[Declarar Situación de Contingencia]
    D --> E[Autoridad de TI evalúa\nimpacto y criticidad]
    E --> F{¿Contingencia aprobada?}
    F -- No --> G[Escalar a nivel directivo]
    G --> Z
    F -- Sí --> H[Habilitar Usuario Técnico del SSO]
    H --> I[Registrar motivo y responsable]
    I --> J[Acciones permitidas]
    J --> J1[Crear o reactivar Super Admin]
    J --> J2[Restaurar roles]
    J --> J3[Corregir configuración]
    J1 --> K[Validar Super Admin activo]
    J2 --> K
    J3 --> K
    K --> L{¿Sistema estabilizado?}
    L -- No --> J
    L -- Sí --> M[Deshabilitar Usuario Técnico]
    M --> N[Registrar cierre]
    N --> Z
```

---

### 5.4 Acciones permitidas

- Crear o reactivar SSO Super Administradores.
- Restaurar roles y asignaciones.
- Corregir configuraciones críticas.

---

### 5.5 Cierre de contingencia

La contingencia se considera cerrada cuando:
- Existe al menos un Super Admin activo.
- El SSO opera con normalidad.
- El Usuario Técnico ha sido deshabilitado.
- Se ha registrado el cierre para auditoría.


---

## 6. Consideraciones Finales

El uso de los tres diagramas permite una comprensión progresiva del modelo:

0. **Diagrama 0:** Diagrama completo (Diagramas 1, 2 y 3 en uno).
1. **Diagrama 1:** explica quién gobierna el SSO y cómo se inicia.
2. **Diagrama 2:** muestra cómo se administran y utilizan los sistemas integrados.
3. **Diagrama 3:** delimita claramente los roles globales y los roles por sistema.
4. **Diagrama 4:** Contigencias.

Este enfoque modular facilita la inclusión del modelo en documentos normativos, manuales técnicos y procesos de auditoría institucional.

