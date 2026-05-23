const express = require('express')

const app = express()

app.use(express.json())

app.post('/disposal', (req, res) => {

    const inventory_id = req.body.inventory_id
    const reason = req.body.reason

    res.json({
        message: `Объект ${inventory_id} списан`,
        reason: reason
    })
})

app.listen(3000, () => {
    console.log('DisposalService running on port 3000')
})