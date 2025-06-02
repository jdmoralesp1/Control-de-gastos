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
            "url": "/Movimientos/Gasto/GetAll"
        },
        "columns": [
            { "data": "fecha", "width": "7%" },
            { "data": "fondoMonetario", "width": "13%" },
            { "data": "nombreComercio", "width": "15%" },
            { "data": "tipoDocumento", "width": "13%" },
            { "data": "numeroDocumento", "width": "9%" },
            {
                "data": "montoTotal",
                "width": "13%",
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return row.montoTotalFormateado;
                    }
                    // Para ordenar y buscar, usa el valor decimal
                    return row.montoTotal;
                }
            },
            { "data": "observaciones", "width": "22%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div>
                            <a href="/Movimientos/Gasto/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                            <i class="bi bi-pencil-square"></i>
                            </a>
                            <a onclick=Delete("/Movimientos/Gasto/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                            <i class="bi bi-trash3-fill"></i>
                            </a>
                        </div>
                    `;
                }, "width": "6%"
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: "Esta seguro de eliminar el gasto",
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