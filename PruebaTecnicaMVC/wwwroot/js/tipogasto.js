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
            "url": "/Admin/TipoGasto/GetAll?inicial=1"
        },
        "columns": [
            { "data": "codigo", "width": "7%", "type": "tg-codigo" },
            {
                "data": "nombre",
                "width": "43%",
                "render": function (data, type, row) {
                    if (typeof data === "string" && data.length > 30) {
                        return data.substring(0, 70) + "...";
                    }
                    return data;
                }
            },
            {
                "data": "descripcion",
                "width": "60%",
                "render": function (data, type, row) {
                    if (typeof data === "string" && data.length > 30) {
                        return data.substring(0, 70) + "...";
                    }
                    return data;
                }
            },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div>
                            <a href="/Admin/TipoGasto/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                            <i class="bi bi-pencil-square"></i>
                            </a>
                            <a onclick=Delete("/Admin/TipoGasto/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                            <i class="bi bi-trash3-fill"></i>
                            </a>
                        </div>
                    `;
                }, "width": "10%"
            }
        ]
    });
}

// Ordenamiento natural para códigos tipo TG1, TG2, TG10, etc.
jQuery.extend(jQuery.fn.dataTable.ext.type.order, {
    "tg-codigo-pre": function (data) {
        // Extrae el número después de TG
        var match = data.match(/^TG(\d+)$/i);
        return match ? parseInt(match[1], 10) : 0;
    }
});


function Delete(url) {
    Swal.fire({
        title: "¿Está seguro de eliminar el tipo de gasto?",
        text: "Este registro no se podrá recuperar",
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
