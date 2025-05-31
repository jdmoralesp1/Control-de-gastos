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
            "url": "/Presupuesto/PresupuestoDetalle/GetAll",
            "dataSrc": function (json) {
                // Actualiza el título con la propiedad recibida
                if (json.titulo) {
                    $('#titulo').text('Detalle Presupuesto' + json.titulo);
                }
                // Devuelve solo el array de datos para DataTables
                return json.data;
            }
        },
        "columns": [
            { "data": "tipoGasto", "width": "40%" },
            { "data": "montoPresupuestado", "width": "40%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div>
                            <a href="/Presupuesto/PresupuestoDetalle/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                            <i class="bi bi-pencil-square"></i>
                            </a>
                            <a onclick=Delete("/Presupuesto/PresupuestoDetalle/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                            <i class="bi bi-trash3-fill"></i>
                            </a>
                        </div>
                    `;
                }, "width": "10%"
            }
        ]
    });
}

function Delete(url) {

    Swal.fire({
        title: "Esta seguro de eliminar el detalle de presupuesto",
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