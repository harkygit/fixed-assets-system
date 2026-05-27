from flask import Flask, jsonify

app = Flask(__name__)

objects = [
    {
        "inventory_id": "OS001",
        "name": "Ноутбук Lenovo ThinkPad T14",
        "cost": 80000
    },
    {
        "inventory_id": "OS002",
        "name": "МФУ HP LaserJet Pro",
        "cost": 25000
    }
]


@app.route('/objects', methods=['GET'])
def get_objects():
    return jsonify(objects)


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5001)
