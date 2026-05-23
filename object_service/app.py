from flask import Flask, jsonify

app = Flask(__name__)

objects = [
    {
        "inventory_id": "OS001",
        "name": "Ноутбук",
        "cost": 80000
    },
    {
        "inventory_id": "OS002",
        "name": "Принтер",
        "cost": 25000
    }
]


@app.route('/objects', methods=['GET'])
def get_objects():
    return jsonify(objects)


if __name__ == '__main__':
    app.run(port=5001)