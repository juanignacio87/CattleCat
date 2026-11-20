# Fase 1A: compilación y stock diario

## Diseño y alcance

`CattleCat.Domain` usa .NET Standard 2.0 y C# 7.3: puede consumirse desde
.NET Framework 4.7.2 y desde .NET moderno sin modernizar el escritorio.
No referencia WPF, EF, Entities ni proveedores de base de datos.
`StockTimeline` recibe movimientos inmutables con fecha, cantidad positiva,
estado activo y efecto positivo, negativo o neutro, para un único lote/categoría.
Agrupa por `Date.Date`, ordena los días y devuelve el primer saldo negativo,
su fecha y saldo; sin conflicto devuelve el saldo final.

El adaptador `CattleCat.Compatibility.LegacyStockOperationCatalog` está aislado
en la carpeta Compatibility de la misma biblioteca para no crear otra capa
completa. Solo él define los IDs productivos: entrada 8, nacimiento 12,
muerte 11 y salida 14. Los demás IDs son neutros. El motor no conoce IDs.
Las constantes públicas del formulario siguen disponibles como alias.

`D_Record` conserva consultas EF6, firmas públicas, exclusión del registro
original y los atajos existentes: propuesta neutra sin retirar movimiento
retorna (true, null, 0); baja de salida/muerte/neutro también retorna ese saldo.
Las simulaciones de alta/edición y baja de entrada/nacimiento usan el motor.
GetStock y GetTotal2 conservan sus consultas y agregaciones de saldos.
El motor no aplica restricciones sanitarias: permanecen en WPF.

Las cantidades no positivas se rechazan como en los formularios existentes.
Esto no es una herramienta de reparación de datos históricos inválidos:
si EF devuelve un movimiento con cantidad no positiva, su conversión al
contrato del motor falla explícitamente. No se inspeccionaron datos reales.

## Compilación sin bases

Se retiran exclusivamente las entradas Content de FarmDB.mdf/FarmDB_log.ldf
del proyecto WPF. Se normalizan también las referencias y la salida Debug
para que no dependan del nombre de la carpeta raíz. Los archivos locales no se eliminan ni se incluyen en Git.
ModelConfig sigue usando .\SQLEXPRESS / CattleCatDemo con seguridad integrada;
no contiene AttachDbFilename y no utiliza esos archivos como mecanismo de conexión.

En PowerShell con Visual Studio 2022 y el Developer Pack 4.7.2 instalados:

```powershell
$msbuild = 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe'
$solutionDir = (Get-Location).Path + '\'
& $msbuild Presentation/Presentation.csproj /t:Restore /p:RestorePackagesConfig=true "/p:SolutionDir=$solutionDir"
dotnet restore CattleCat.Domain.Tests/CattleCat.Domain.Tests.csproj
& $msbuild Presentation/Presentation.csproj /t:Build /p:Configuration=Debug
dotnet test CattleCat.Domain.Tests/CattleCat.Domain.Tests.csproj --no-restore
```

No ejecutar la solución completa para esta fase: incluye PhoneInterface.
Los tests net472 usan NUnit y referencian solo el dominio; no instancian Model
ni necesitan SQL Server o MDF. Las pruebas de reemplazo/exclusión trabajan
con listas en memoria, no acreditan integración contra EF o una base real.

## Riesgos conservados y Fase 1B

- Validación y escritura siguen separadas. Dos clientes pueden validar sobre
  el mismo saldo y guardar resultados incompatibles. Centralizar el caso de
  uso y diseñar transacción/aislamiento es trabajo de Fase 1B.
- Las rutas públicas de guardado aún pueden omitir validación WPF.
- La cronología es diaria: una salida antes de una entrada del mismo día
  puede ser válida si el cierre diario no queda negativo.
- Conteo no ajusta stock; los tratamientos no cambian saldo.
- Se conserva Int32, incluido el desbordamiento histórico del saldo acumulado;
  Sum diario detecta overflow. Antes de ampliar rangos, acordar política y
  unificar aritmética verificada. Los extremos patológicos de desbordamiento
  entre efectos agregados requieren revisión antes de admitir grandes saldos.
- La baja de lote sigue comprobando stock total, no saldo por categoría.
- No se cambian catálogos, esquema, conexiones, pantallas ni reglas sanitarias.
- La corrección de nulos mueve la lectura de Id y SelectedDate.Value después
  de los controles existentes; conserva mensajes. No se añade infraestructura
  de pruebas WPF para ese cambio local.
