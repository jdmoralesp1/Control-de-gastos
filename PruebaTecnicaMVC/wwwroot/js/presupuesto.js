let datatable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    datatable = $('#tblDatos').DataTable({
        "order": [[0, "desc"]],
        "language": {
            "lengthMenu": "Mostrar _MENU_ Registros Por Pagina",
            "zeroRecords": "Ningun Registro",
            "info": "Mostrando página _PAGE_ de _PAGES_",
            "infoEmpty": "No hay registros",
            "infoFiltered": "(Filtrado de un total de _MAX_ registros)",
            "search": "Buscar",
            "paginate": {
                "first": "Primero",
                "last": "Último",
                "next": "Siguiente",
                "previous": "Anterior"
            }
        },
        "ajax": {
            "url": "/Presupuesto/Presupuesto/GetAll"
        },
        "columns": [
            { "data": "anio", "width": "10%" },
            { "data": "mes", "width": "15%" },
            {
                "data": "montoTotal",
                "width": "15%",
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return row.montoTotalFormateado;
                    }
                    // Para ordenar y buscar, usa el valor decimal
                    return row.montoTotal;
                }
            },
            {
                "data": "fechaCreacion",
                "type": "date",
                "width": "20%",
                "render": function (data, type, row) {
                    if (!data) return "";
                    // Si el formato no es ISO, intenta convertirlo
                    let dateStr = data;
                    // Si el formato es "DD-MM-YYYY HH:mm", conviértelo a "YYYY-MM-DDTHH:mm"
                    if (/^\d{2}-\d{2}-\d{4} \d{2}:\d{2}$/.test(data)) {
                        const [d, m, y, h, min] = data.match(/\d+/g);
                        dateStr = `${y}-${m}-${d}T${h}:${min}`;
                    }
                    var dateObj = new Date(dateStr);
                    if (isNaN(dateObj.getTime())) return ""; // Si la fecha no es válida, retorna vacío
                    if (type === 'display' || type === 'filter') {
                        let day = String(dateObj.getDate()).padStart(2, '0');
                        let month = String(dateObj.getMonth() + 1).padStart(2, '0');
                        let year = dateObj.getFullYear();
                        let hora = String(dateObj.getHours()).padStart(2, '0');
                        let minuto = String(dateObj.getMinutes()).padStart(2, '0');
                        return `${day}-${month}-${year} ${hora}:${minuto}`;
                    }
                    return data;
                }

            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div>
                            <a href="/Presupuesto/Presupuesto/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                            <i class="bi bi-pencil-square"></i> Editar
                            </a>
                            <a onclick=Delete("/Presupuesto/Presupuesto/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                            <i class="bi bi-trash3-fill"></i> Eliminar
                            </a>
                            <a onclick="modificarDetallePresupuesto(${data})" class="btn btn-primary text-white" style="cursor:pointer">
                            <i class="bi bi-arrow-up-right-square-fill"></i> Detalle Presupuesto
                            </a>
                        </div>
                    `;
                }, "width": "20%"
            }
        ]
    });
}


function modificarDetallePresupuesto(id) {
    $.ajax({
        url: '/Presupuesto/Presupuesto/SetTempDataPresupuestoId',
        type: 'POST',
        data: { id: id },
        success: function () {
            window.location.href = '/Presupuesto/PresupuestoDetalle/';
        },
        error: function () {
            toastr.error('No se pudo preparar el detalle del presupuesto.');
        }
    });
}


function Delete(url) {
    Swal.fire({
        title: "Esta seguro de eliminar el presupuesto",
        text: "Este registro no se podra recuperar",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Sí, eliminar",
        cancelButtonText: "Cancelar"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.succeeded) {
                        toastr.success(data.message);
                        datatable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}