# Fase 1C: operaciones limitadas por existencias

## Causa y alcance
El motor anterior solo comprobaba el saldo diario. Una salida retroactiva podía dejar
saldo positivo y, sin embargo, invalidar un tratamiento o conteo posterior.
La regla se aplica al guardar mediante RecordWriter, no solo en formularios.
No cambia el esquema, el proveedor, la conexión ni los datos existentes.

## Catálogo por ID
| IDs | Operación | Comportamiento |
| --- | --- | --- |
| 8, 12 | Entrada, Nacimiento | Incrementan existencias |
| 11, 14 | Muerte, Salida | Reducen existencias sin saldo negativo |
| 5 | Conteo | Cantidad exactamente igual al stock disponible; no ajusta |
| 1, 2, 3, 4 | Aftosa, Antiparasitario, Brucelosis, Castración | Animales existentes |
| 6, 7, 9, 10 | Destete, Ecografía, Inseminación, I.R.9 | Animales existentes |
| 13, 15, 16, 17, 18 | Reproductiva, Queratoconjuntivitis, Sarna, Antidiarreico, Carbunclo | Animales existentes |

Animales existentes significa saldo neutro, stock estrictamente positivo y cantidad
positiva menor o igual al disponible. No se incorporan reglas veterinarias nuevas.
Los IDs desconocidos se rechazan explícitamente: no se les asigna significado por nombre.
El catálogo procede de los IDs y controles del código existente; no se consultó la base.

## Convención intradiaria
No hay orden persistido dentro de una fecha. Se ignora la hora y se valida:
1. Todas las entradas y nacimientos del día.
2. Todos los conteos y operaciones sobre animales existentes.
3. Todas las salidas y muertes, comprobando el saldo de cierre.

Los conteos y tratamientos no consumen animales entre sí. Un tratamiento de 35 y una
salida de 12 el mismo día son compatibles si había 35 antes de las salidas.
El orden de la colección no altera el resultado. Si hay varios conflictos en un día,
se informa de forma determinista por ID de operación y cantidad; entre grupos se
prioriza fecha y después el orden de lote/categoría.

## Ejemplo de regresión
Entrada 35 el 06/11/2026; salida 12 el 10/11; antidiarreico 35 el 13/11:
conflicto de tratamiento el 13/11, cantidad 35, disponibles 23. Una entrada de 12
antes del tratamiento lo permite; una entrada posterior no lo justifica.

## Escrituras y compatibilidad
Altas individuales y múltiples, ediciones y bajas validan toda la cronología activa
de cada lote/categoría afectado, excluyendo el original al editar o eliminar.
Cambiar de lote/categoría valida origen y destino. El lote identifica su establecimiento.
También se valida al retirar una salida: aumentar el saldo puede invalidar un conteo.
Se conserva una sesión/contexto, una transacción Serializable, un SaveChanges y un Commit.
Un rechazo o excepción abandona la transacción sin cambios parciales.
No existe reactivación: altas con ID e intentos de editar registros inactivos se rechazan.

El constructor StockMovement con StockEffect conserva las consultas históricas de
solo saldo y sus pruebas de caracterización. Las escrituras usan exclusivamente el
constructor con StockOperationBehavior. Los helpers antiguos de lectura con tuplas
no constituyen autorización de escritura; las pantallas ya no los usan como validación.
No se duplicó el algoritmo en presentación ni se alteraron restricciones de categorías
u operaciones incompatibles existentes en los formularios.

## Resultados y mensajes
Se distinguen saldo negativo, operación superior al disponible, conteo distinto,
cantidad/datos inválidos, catálogo desconocido y error técnico. Los conflictos llevan
fecha, ID de operación, cantidad, stock y lote/categoría; los mensajes españoles se
centralizan en Logic.RecordWriteMessages sin excepciones ni SQL.
Una discrepancia de conteo ya no ofrece guardar igualmente ni produce ajuste automático.
Se mantiene el rechazo previo de cantidades cero o negativas, incluso para conteos.

## Verificación y precauciones
Pruebas puras de dominio y sesiones transaccionales en memoria, sin SQL Server.
Se conservan las 109 pruebas previas; las expectativas de neutras sin stock se adaptan
a la regla nueva y los fixtures de rollback usan conteos válidos para alcanzar el fallo.
Datos históricos con tratamientos/conteos inválidos pueden bloquear futuras escrituras
sobre su grupo. No se reparan automáticamente. La prueba funcional WPF queda para el usuario.
No se ejecuta la aplicación, ningún inicializador ni comandos EF durante la validación.
