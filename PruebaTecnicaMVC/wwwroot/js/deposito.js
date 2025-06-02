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
            "url": "/Movimientos/Deposito/GetAll"
        },
        "columns": [
            {
                "data": "fecha",
                "type": "date",
                "width": "20%",
                "render": function (data, type, row) {
                    if (!data) return "";
                    var dateObj = new Date(data);
                    // Si el tipo es 'display', muestra como dd-MM-yyyy, si no, retorna el valor original para ordenar/filtrar
                    if (type === 'display' || type === 'filter') {
                        let day = String(dateObj.getDate()).padStart(2, '0');
                        let month = String(dateObj.getMonth() + 1).padStart(2, '0');
                        let year = dateObj.getFullYear();
                        return `${day}-${month}-${year}`;
                    }
                    return data;
                }
            },
            { "data": "fondoMonetario", "width": "20%" },
            {
                "data": "monto",
                "width": "20%",
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return row.montoFormateado;
                    }
                    // Para ordenar y buscar, usa el valor decimal
                    return row.monto;
                }
            },
            { "data": "observaciones", "width": "30%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div>
                            <a href="/Movimientos/Deposito/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                            <i class="bi bi-pencil-square"></i>
                            </a>
                            <a onclick=Delete("/Movimientos/Deposito/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
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
        title: "Esta seguro de eliminar el deposito",
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