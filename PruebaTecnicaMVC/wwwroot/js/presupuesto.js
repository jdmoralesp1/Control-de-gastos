let datatable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    datatable = $('#tblDatos').DataTable({
        "language": {
            "lengthMenu": "Mostrar _MENU_ Registros Por Pagina",
            "zeroRecords": "Ningun Registro",
            "info": "Mostrando página _PAGE_ de _PAGES_",
            "infoEmpty": "No hay registros",
            "infoFiltered": "(filtered from _MAX_ total registros)",
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
            { "data": "anio", "width": "20%" },
            { "data": "mes", "width": "20%" },
            { "data": "montoTotal", "width": "20%" },
            { "data": "fechaCreacion", "width": "20%" },
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