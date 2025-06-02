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
            "url": "/Admin/FondoMonetario/GetAll?inicial=1"
        },
        "columns": [
            {
                "data": "nombre",
                "width": "10%",
                "render": function (data, type, row) {
                    if (typeof data === "string" && data.length > 30) {
                        return data.substring(0, 30) + "...";
                    }
                    return data;
                }
            },
            {
                "data": "tipo", "width": "10%",
                "render": function (data, type, row) {
                    if (typeof data === "string" && data.length > 30) {
                        return data.substring(0, 30) + "...";
                    }
                    return data;
                }
            },
            { "data": "numeroCuenta", "width": "20%" },
            {
                "data": "saldoActual",
                "width": "20%",
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return row.saldoActualFormateado;
                    }
                    // Para ordenar y buscar, usa el valor decimal
                    return row.saldoActual;
                }
            },
            { "data": "fechaCreacion", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div>
                            <a href="/Admin/FondoMonetario/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                            <i class="bi bi-pencil-square"></i>
                            </a>
                            <a onclick=Delete("/Admin/FondoMonetario/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
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
        title: "Esta seguro de eliminar el fondo monetario",
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



    //swal({
    //    title: "Esta seguro de eliminar el fondo monetario",
    //    text: "Este registro no se podra recuperar",
    //    icon: "warning",
    //    buttons: true,
    //    dangerMode: true
    //}).then((borrar) => {
    //    if (borrar) {
    //        $.ajax({
    //            type: "DELETE",
    //            url: url,
    //            success: function (data) {
    //                if (data.succeeded) {
    //                    toastr.success(data.message);
    //                    datatable.ajax.reload();
    //                }
    //                else {
    //                    toastr.error(data.message);
    //                }
    //            }
    //        });
    //    }
    //});
}