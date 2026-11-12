# Control local de visitas (prototipo inicial)

Aplicación WinForms en C# para .NET Framework 3.5, configurada para compilar como x86 y orientada a Windows de 32 bits. No necesita Internet ni una base de datos de servidor.

## Compilar

Abrir `VisitControl.csproj` con Visual Studio compatible con proyectos .NET 3.5 y compilar la configuración **Release | x86**. El resultado `ControlVisitas.exe` se genera en `bin/Release`.

## Funcionalidad inicial

- Alta y edición de personas con DNI, nombre, apellido, función/cargo (Visitante por defecto), fotografía seleccionada desde archivo y restricción con motivo/fechas.
- Popup de persona con foto, nombre, DNI y función. Si la restricción está vigente, muestra además el aviso rojo, motivo y vencimiento; la pantalla no presenta esos detalles para personas sin restricción activa.
- Registro local por DNI para probar el flujo mientras se valida el SDK.
- Primera marcación permitida del día: Entrada; segunda: Salida. Marcas posteriores se registran como Adicional, sin alternar silenciosamente entre entrada y salida.
- Los intentos denegados se conservan como intentos y no cuentan como entrada/salida.
- Búsqueda sin distinguir mayúsculas/minúsculas, rango de fechas y exportación CSV con los mismos filtros.
- Datos XML y fotos guardados en `%APPDATA%\ControlVisitas`.

## Pendiente antes de operación real

La carpeta subida contiene el wrapper administrado `libzkfpcsharp.dll`, pero no se ven en el árbol las DLL nativas del motor ni los controladores necesarios. La lectura biométrica todavía no está conectada a la interfaz; no se debe presentar el registro por DNI como verificación biométrica. Para integrar el ZK9500 hacen falta las dependencias nativas x86 completas, la confirmación de compatibilidad con el Windows objetivo y pruebas con el dispositivo. El evento de una huella no registrada deberá abrir el aviso **HUELLA SIN REGISTRAR** al conectar el lector.

La captura desde cámara web también queda pendiente; esta versión permite seleccionar una foto existente. El almacenamiento XML es local, pero no está cifrado ni incluye autenticación/auditoría administrativa. Es un prototipo funcional para validar pantallas y reglas, no debe usarse como sistema oficial ni como mecanismo de autorización de ingreso hasta completar pruebas, controles de acceso, respaldo y revisión institucional.

## Regla de visita usada

El horario de visitas (08:00–15:00) no determina el tipo de marcación. La clasificación usa el orden de marcación del día, de modo que alguien que entra a las 08:00 y sale a las 12:00 queda correctamente como Entrada y Salida. Una tercera marca no se interpreta automáticamente como un nuevo ingreso; queda como adicional para evitar inferir una reentrada sin una regla definida.
