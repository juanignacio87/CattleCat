# Fase 1B: escritura y cronología en una transacción

## Diagnóstico previo

En 1b61a1a existían estos caminos públicos:

| Dalc | Logic | Consumidor WPF | Protección anterior |
|---|---|---|---|
| d_Record | LRecord | Sin consumidor actual | Add + SaveChanges sin validación |
| AddRecords | LRecords | UCOperation.btnAdd_Click | Transacción por defecto solo para AddRange/SaveChanges; validación previa en WPF |
| UpdateOne | Update | EditOp.btnAdd_Click | Find + asignaciones + SaveChanges sin validación definitiva |
| Delete | Delete | UCGrid.btnDelete_Click | CanDeactivateRecord y SaveChanges separados |

La búsqueda de Records.Add/AddRange/Remove, Activo y SaveChanges no encontró
otros caminos actuales de escritura de Record en Presentation, Logic o Dalc.
Las escrituras de usuarios, establecimientos y lotes son otros casos de uso y
no se rediseñan aquí. Model expone DbSet públicos: SQL directo u otro código
que use Model sin estos servicios no queda protegido por una restricción nueva
ni por este cambio. El alcance es cerrar los caminos de aplicación encontrados.

D_Record ya delegaba en StockTimeline para validar una propuesta y la baja de
un ingreso. Esas funciones siguen disponibles como prevalidaciones, nunca como
permiso definitivo para guardar.

## Diseño

- RecordWriter coordina altas individuales/múltiples, edición y baja lógica.
- EfRecordWriteSession es el único adaptador productivo de escritura y crea
  un Model nuevo por comando. No reutiliza objetos rastreados por la interfaz.
- Comienza BeginTransaction(IsolationLevel.Serializable) antes de consultar
  el registro original, referencias o movimientos. Lecturas de validación,
  cambios, SaveChanges y Commit usan el mismo contexto y transacción.
- Todos los rechazos retornan antes de SaveChanges. Dispose de una transacción
  no confirmada realiza rollback. Una excepción también sale por Dispose;
  el contexto se descarta y no contamina el siguiente intento.
- Las altas múltiples se validan completas, con sus efectos combinados por
  lote/categoría, antes de agregar entidades al contexto. Solo hay un guardado
  y un commit por comando, no uno por fila.
- Edición excluye el ID original y aplica el reemplazo una vez; valida origen
  y destino. Se devuelve la primera fecha de conflicto entre grupos afectados.
- StockTimeline conserva el algoritmo diario. La clasificación sigue en
  LegacyStockOperationCatalog; no se cambian los IDs de la base.

Se exige cantidad positiva, fecha no futura, usuario/lote/establecimiento
existentes y activos, y categoría/operación existentes. Los estados distintos
de 1 no se consideran activos. La misma validación de campos y referencias
se aplica a la baja lógica: un registro ligado a un padre desactivado queda
bloqueado con un resultado explícito; no se reactiva ni corrige automáticamente.
Desactivar una salida u operación neutra no requiere recalcular una reducción
de stock; retirar un ingreso sí. En todos los casos se hace dentro de transacción.

GetRecordToEdit devuelve una entidad sin seguimiento. El editor prepara un
candidato separado y conserva la versión cargada cuando el guardado se rechaza.
Las prevalidaciones WPF sanitarias y de cantidad de animales tratados siguen
existiendo; no se trasladan ni amplían como parte de estas reglas cronológicas.

## Contratos y mensajes

D_Record y L_Record ofrecen TryCreate, TryCreateMany, TryUpdate y TryDeactivate
con RecordWriteResult. Distingue éxito, petición/cantidad inválida, fecha futura,
referencias inexistentes/inactivas, registro no disponible, conflicto con fecha
 y saldo, y fallo técnico. Se conserva la firma de los métodos antiguos: los
void delegan y lanzan RecordWriteException tipada ante rechazo; Delete devuelve
false ante rechazo de negocio y lanza la excepción tipada ante fallo técnico.
Ninguna ruta antigua omite RecordWriter.

WPF consume resultados explícitos y mensajes de RecordWriteMessages, sin abrir
transacciones ni mostrar detalles crudos de proveedor. Las excepciones técnicas
se registran con Trace; los formularios conservan datos ante un rechazo.

## Aislamiento y concurrencia

Serializable conserva los bloqueos de lectura hasta terminar la transacción y
protege frente a cambios/filas fantasma en los conjuntos usados para calcular
stock. SQL Server puede usar bloqueos de rango o bloqueos más amplios según el
plan e índices. Dos salidas que compiten no pueden confirmar juntas basándose
en el mismo saldo anterior si ambas usan estos caminos.

Se elige esta opción por simplicidad, sin bloqueos distribuidos ni esquema nuevo.
Puede producir bloqueos prolongados o deadlocks, especialmente en conversiones
lectura/escritura. SQL Server aborta una transacción víctima; el cliente recibe
TechnicalError y no se reintenta automáticamente. La ordenación de grupos
reduce algunos órdenes opuestos de bloqueo, pero no elimina deadlocks.

La política no evita sobrescribir una edición obsoleta si el resultado sigue
siendo válido: detectar cambios perdidos requeriría un contrato de versión o
comparación adicional. Tampoco garantiza que todos los padres permanezcan
activos después del commit si otro caso de uso los desactiva más tarde.
Una desconexión durante Commit puede dejar el resultado de confirmación incierto;
se deben actualizar los datos antes de repetir, para no duplicar un alta.

## Pruebas y límites de validación

Se conservan las 40 pruebas puras de Fase 1A. Dalc.Tests usa las mismas versiones
fijadas de Test SDK, NUnit y adaptador; no se actualiza ningún paquete existente.
Las nuevas pruebas ejercitan RecordWriter con una sesión transaccional en memoria:
altas, lotes de comandos, referencias, fechas, edición/origen/destino, bajas,
rollback ante rechazo y fallas antes/después de SaveChanges y en Commit, y
reutilización después de un fallo.

La prueba de dos tareas usa transacciones serializadas en el doble de pruebas.
Demuestra el contrato de coordinación, NO los bloqueos reales del proveedor.
No se ejecuta una prueba de concurrencia SQL: no hay una infraestructura de
base aislada, aprovisionada y recuperable dentro de este alcance sin tocar el
esquema ni dejar escrituras. No se usa CattleCatDemo ni los MDF/LDF para tests.
La garantía SQL se revisa por inspección de la frontera transaccional y debe
validarse posteriormente en una base exclusiva de integración.
No se ejecuta WPF, migraciones, seeders ni inicializadores en esta tarea.
Los límites Int32 del motor permanecen; datos históricos inválidos o errores de
representación producen fallo técnico y rollback, no una reparación silenciosa.

## Validación y prueba manual propuesta

```powershell
dotnet test CattleCat.Domain.Tests/CattleCat.Domain.Tests.csproj
dotnet test Dalc.Tests/Dalc.Tests.csproj
# Con MSBuild de Visual Studio y Developer Pack 4.7.2:
MSBuild.exe Presentation/Presentation.csproj /t:Build /p:Configuration=Debug
git diff --check
```

La restauración clásica necesita RestorePackagesConfig=true y SolutionDir
apuntando a la raíz, como se documenta en Fase 1A. Compilar el proyecto WPF
incluye Domain, Entities, Dalc y Logic; los dos comandos test compilan sus tests.
No compilar indiscriminadamente la solución, que conserva PhoneInterface.

Tras revisión y autorización para probar con datos ficticios:

1. Entrada 20, conteo 20, salida 5: saldos 20/20/15.
2. Rechazar salida 16; confirmar que no cambia el historial.
3. Rechazar reducción de entrada a 4 o fecha posterior a la salida.
4. Editar entrada a 25; comprobar saldo 20 sin duplicados.
5. Bloquear baja de ingreso necesario; permitir baja de conteo y conservar fila.
6. Cambiar lote/categoría; comprobar origen y destino, y padres inactivos.
7. En base exclusiva de integración, iniciar dos clientes con salidas de 15
   sobre stock 20: no deben confirmar ambas; admitir conflicto o deadlock
   controlado, comprobar saldo final y ausencia de escrituras parciales.
8. Cerrar WPF y verificar las filas con SELECT sin borrar físicamente datos.
