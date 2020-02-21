function validateToDoItemTimestamp() {
    var context = getContext();
    var request = context.getRequest();

    // item to be created in the current operation
    var itemToCreate = request.getBody();

    // validate properties
    if (!('timestamp' in itemToCreate)) {
        var ts = new Date();
        itemToCreate['timestamp'] = ts.getTime();
    }

    // update the item that will be created
    request.setBody(itemToCreate);
}